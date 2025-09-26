using System.Collections.ObjectModel;
using System.Data;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Repositories;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.RepositoryResults;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.SqlFiltering;
using Dapper;
using Npgsql;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared;

internal abstract class PostgresDbRepository<TEntity, TRow, TKey, TFilter> : IRepository<TEntity, TKey, TFilter> where TEntity : class
{
    private readonly ISqlFilterBuilder _sqlFilterBuilder;
    private readonly string _selectStatement;
    private readonly string _insertStatement;
    private readonly string _updateStatement;
    private readonly string _deleteStatement;
    private readonly string _cutoffStatement;

    protected PostgresDbRepository(ISqlFilterBuilder sqlFilterBuilder)
    {
        _sqlFilterBuilder = sqlFilterBuilder;
        _selectStatement = BuildSelect();
        _insertStatement = BuildInsert();
        _updateStatement = BuildUpdate();
        _deleteStatement = BuildDelete();
        _cutoffStatement = BuildCutoffStatement();
    }

    protected abstract string ConnectionString { get; }
    protected abstract string TableName { get; }
    protected abstract string KeyColumnName { get; }
    protected abstract DbType KeyColumnDatabaseType { get; }
    protected abstract Func<TRow, TKey> KeySelector { get; }
    protected abstract IReadOnlyList<(string ColumnName, string PropertyName)> ColumnMappings { get; }
    protected abstract Func<TRow, TEntity> ToEntity { get; }
    protected abstract Func<TEntity, TRow> ToRow { get; }
    protected abstract Func<TFilter?, List<SqlFilterBuilder.FilterMapping>> ToFilterMappings { get; }

    public async Task<GetResult<TEntity>> GetAsync(TKey identifier, CancellationToken cancellationToken)
    {
        var sql = $"""
                   {_selectStatement}
                   where {KeyColumnName} = @identifier
                   """;
        var command = new CommandDefinition(
            sql,
            new { identifier },
            cancellationToken: cancellationToken);
        await using var connection = new NpgsqlConnection(ConnectionString);
        var row = await connection.QuerySingleOrDefaultAsync<TRow>(command);
        return row is null
            ? new GetResult<TEntity>.NotFound()
            : new GetResult<TEntity>.Success(ToEntity.Invoke(row));
    }

    public async Task<CreateResult> CreateAsync(TEntity entityToCreate, CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(_insertStatement, ToRow.Invoke(entityToCreate), cancellationToken: cancellationToken);
        try
        {
            await using var connection = new NpgsqlConnection(ConnectionString);
            await connection.ExecuteAsync(command);
            return new CreateResult.Success();
        }
        catch (PostgresException postgresException) when (postgresException.SqlState is Constants
                                                              .PostgresExceptionSqlStates.DuplicateKey)
        {
            return new CreateResult.AlreadyExists();
        }
    }

    public async Task<UpdateResult> UpdateAsync(TEntity entityToUpdate, CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(_updateStatement, ToRow(entityToUpdate), cancellationToken: cancellationToken);
        await using var connection = new NpgsqlConnection(ConnectionString);
        var effectedRows = await connection.ExecuteAsync(command);
        return effectedRows != 1
            ? new UpdateResult.NotFound()
            : new UpdateResult.Success();
    }

    public async Task<DeleteResult> DeleteAsync(TKey identifier, CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(_deleteStatement, new { identifier }, cancellationToken: cancellationToken);
        await using var connection = new NpgsqlConnection(ConnectionString);
        var effectedRows = await connection.ExecuteAsync(command);
        return effectedRows != 1
            ? new DeleteResult.NotFound()
            : new DeleteResult.Success();
    }

    public IAsyncEnumerable<TEntity> StreamAllAsync(int batchSize, TFilter filter, CancellationToken cancellationToken)
    => KeysetPager.StreamAsync<TRow, TKey, TEntity>(
    fetchCutoffAsync: FetchCutoffAsync,
    fetchPageAsync: (after, cutoff, fetchBatchSize, fetchCancellationToken) =>
    FetchPageUpToCutoffAsync(after, cutoff, fetchBatchSize, filter, fetchCancellationToken),
    keySelector: KeySelector,
    map: row => ToEntity.Invoke(row),
    batchSize: batchSize,
    cancellationToken: cancellationToken);

    private string BuildSelect()
    {
        var columnPropertyMappings = string.Join(
            ", ",
            ColumnMappings.Select(mapping
                => $"{mapping.ColumnName} as {mapping.PropertyName}"
            )
        );
        return $"select {columnPropertyMappings} from {TableName}";
    }

    private string BuildInsert()
    {
        var columns = string.Join(", ", ColumnMappings.Select(mapping => mapping.ColumnName));
        var properties = string.Join(", ", ColumnMappings.Select(mapping => $"@{mapping.PropertyName}"));
        return $"insert into {TableName} ({columns}) values ({properties});";
    }

    private string BuildUpdate()
    {
        var keyColumnMapping = ColumnMappings.Single(mapping => mapping.ColumnName == KeyColumnName);
        var noKeyColumnMappings = ColumnMappings.Except([keyColumnMapping]);
        var columnPropertyMappings =
            string.Join(", ", noKeyColumnMappings.Select(mapping => $"{mapping.ColumnName} = @{mapping.PropertyName}"));
        return $"update {TableName} set {columnPropertyMappings} where {KeyColumnName} = @{keyColumnMapping.PropertyName}";
    }

    private string BuildDelete()
        => $"delete from {TableName} where {KeyColumnName} = @Identifier";

    private string BuildCutoffStatement()
        => $"select max({KeyColumnName}) from {TableName}";

    private async Task<TKey?> FetchCutoffAsync(CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(_cutoffStatement, cancellationToken: cancellationToken);
        await using var connection = new NpgsqlConnection(ConnectionString);
        return await connection.ExecuteScalarAsync<TKey>(command);
    }

    private async Task<ReadOnlyCollection<TRow>> FetchPageUpToCutoffAsync(
        TKey? after,
        TKey cutoff,
        int limit,
        TFilter filter,
        CancellationToken cancellationToken)
    {
        var (whereClause, filterParameters) = _sqlFilterBuilder.Build(ToFilterMappings.Invoke(filter));
        var pageSql = $"""
                       {_selectStatement}
                       {whereClause}
                         and (@After is null or {KeyColumnName} > @After)
                         and {KeyColumnName} <= @Cutoff
                       order by {KeyColumnName}
                       limit @Limit
                       """;

        var parameters = new DynamicParameters();
        foreach (var filterParameter in filterParameters)
        {
            parameters.Add(filterParameter.Key, filterParameter.Value);
        }
        parameters.Add("@After", after, KeyColumnDatabaseType);
        parameters.Add("@Cutoff", cutoff);
        parameters.Add("@Limit", limit);

        var command = new CommandDefinition(
            pageSql,
            parameters,
            cancellationToken: cancellationToken);
        await using var connection = new NpgsqlConnection(ConnectionString);
        return await connection.QueryAsync<TRow>(command)
            .ContinueWith(task => task.Result
                    .ToList()
                    .AsReadOnly(),
                cancellationToken);
    }
}
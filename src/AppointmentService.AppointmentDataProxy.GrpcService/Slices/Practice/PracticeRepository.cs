using System.Collections.ObjectModel;
using System.Data;
using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.RepositoryResults;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Settings;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.SqlFiltering;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.Practice;

internal interface IPracticeRepository
{
    Task<GetResult<Protos.Practice>> GetAsync(string institutionCode, CancellationToken cancellationToken);
    Task<CreateResult> CreateAsync(Protos.Practice practice, CancellationToken cancellationToken);
    Task<UpdateResult> UpdateAsync(Protos.Practice practice, CancellationToken cancellationToken);
    Task<DeleteResult> DeleteAsync(string institutionCode, CancellationToken cancellationToken);

    IAsyncEnumerable<Protos.Practice> StreamAllAsync(int batchSize,
        PracticeFilter filter,
        CancellationToken cancellationToken);
}

internal class PracticeRepository(
    IOptions<ConnectionStringsSettings> connectionStringsSettings,
    IIntFilterOptionsMapper<Int32Filter> intFilterOptionsMapper,
    IStringFilterOptionsMapper<StringFilter> stringFilterOptionsMapper,
    ISqlFilterBuilder sqlFilterBuilder) : IPracticeRepository
{
    private readonly string _connectionString = connectionStringsSettings.Value.CentralDatabase;

    public async Task<GetResult<Protos.Practice>> GetAsync(string institutionCode, CancellationToken cancellationToken)
    {
        const string sql = """
                           select institution_code as institutionCode, name as Name
                           from Practice 
                                    where institution_code = @institutionCode
                           """;
        var command = new CommandDefinition(sql, new { institutionCode }, cancellationToken: cancellationToken);
        await using var connection = new NpgsqlConnection(_connectionString);
        var row = await connection.QuerySingleOrDefaultAsync<PracticeRow>(command);
        return row is null
            ? new GetResult<Protos.Practice>.NotFound()
            : new GetResult<Protos.Practice>.Success(row.ToPractice());
    }

    public async Task<CreateResult> CreateAsync(Protos.Practice practice, CancellationToken cancellationToken)
    {
        const string insertSql = """
                                                 insert into Practice (institution_code, name)
                                                 values (@institutionCode, @Name);
                                 """;
        var command = new CommandDefinition(insertSql, new PracticeRow(practice), cancellationToken: cancellationToken);
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.ExecuteAsync(command);
            return new CreateResult.Success();
        }
        catch (PostgresException postgresException) when (postgresException.SqlState is Constants
                                                              .PostgresExceptionSqlStates.DuplicateKey)
        {
            return new CreateResult.AlreadyExists();
        }
    }

    public async Task<UpdateResult> UpdateAsync(Protos.Practice practice, CancellationToken cancellationToken)
    {
        const string updateSql = """
                                                 update Practice set
                                                     name = @Name
                                                 where institution_code = @institutionCode
                                 """;
        var command = new CommandDefinition(updateSql, new PracticeRow(practice), cancellationToken: cancellationToken);
        await using var connection = new NpgsqlConnection(_connectionString);
        var effectedRows = await connection.ExecuteAsync(command);
        return effectedRows != 1
            ? new UpdateResult.NotFound()
            : new UpdateResult.Success();
    }

    public async Task<DeleteResult> DeleteAsync(string institutionCode, CancellationToken cancellationToken)
    {
        const string sql = "delete from Practice where institution_code = @institutionCode";
        var command = new CommandDefinition(sql, new { institutionCode }, cancellationToken: cancellationToken);
        await using var connection = new NpgsqlConnection(_connectionString);
        var effectedRows = await connection.ExecuteAsync(command);
        return effectedRows != 1
            ? new DeleteResult.NotFound()
            : new DeleteResult.Success();
    }

    public IAsyncEnumerable<Protos.Practice> StreamAllAsync(int batchSize,
        PracticeFilter filter,
        CancellationToken cancellationToken)
        => KeysetPager.StreamAsync<PracticeRow, string, Protos.Practice>(
            fetchCutoffAsync: FetchCutoffAsync,
            fetchPageAsync: (after, cutoff, fetchBatchSize, fetchCancellationToken) =>
                FetchPageUpToCutoffAsync(after, cutoff, fetchBatchSize, filter, fetchCancellationToken),
            keySelector: row => row.InstitutionCode,
            map: row => row.ToPractice(),
            batchSize: batchSize,
            cancellationToken: cancellationToken);

    private async Task<string?> FetchCutoffAsync(CancellationToken cancellationToken)
    {
        const string cutoffSql = "select max(institution_code) from Practice";
        var command = new CommandDefinition(cutoffSql, cancellationToken: cancellationToken);
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<string?>(command);
    }

    private async Task<ReadOnlyCollection<PracticeRow>> FetchPageUpToCutoffAsync(
        string? after,
        string cutoff,
        int limit,
        PracticeFilter filter,
        CancellationToken cancellationToken)
    {
        var (whereClause, filterParameters) = sqlFilterBuilder.Build(ToFilterMappings(filter));
        var pageSql = $"""
                       select institution_code as institutionCode, name as Name
                       from Practice
                       {whereClause}
                         and (@After is null or institution_code > @After)
                         and institution_code <= @Cutoff
                       order by institution_code
                       limit @Limit
                       """;

        var parameters = new DynamicParameters();
        foreach (var filterParameter in filterParameters)
        {
            parameters.Add(filterParameter.Key, filterParameter.Value);
        }
        parameters.Add("@After", after, DbType.String);
        parameters.Add("@Cutoff", cutoff);
        parameters.Add("@Limit", limit);

        var command = new CommandDefinition(
            pageSql,
            parameters,
            cancellationToken: cancellationToken);
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<PracticeRow>(command)
            .ContinueWith(task => task.Result
                    .ToList()
                    .AsReadOnly(),
                cancellationToken);
    }

    private List<SqlFilterBuilder.FilterMapping> ToFilterMappings(PracticeFilter filter)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (filter is null)
            return [];

        var mappings = new List<SqlFilterBuilder.FilterMapping>();

        if (stringFilterOptionsMapper.Map(filter.InstitutionCode) is {} institutionCodeMapping)
            mappings.Add(new SqlFilterBuilder.FilterMapping("institution_code", institutionCodeMapping));

        if (stringFilterOptionsMapper.Map(filter.Name) is { } nameMapping)
            mappings.Add(new SqlFilterBuilder.FilterMapping("name", nameMapping));

        return mappings;
    }
}
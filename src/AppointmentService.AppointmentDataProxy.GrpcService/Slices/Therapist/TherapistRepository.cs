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

namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.Therapist;

internal interface ITherapistRepository
{
    Task<GetResult<Protos.Therapist>> GetAsync(int id, CancellationToken cancellationToken);
    Task<CreateResult> CreateAsync(Protos.Therapist therapist, CancellationToken cancellationToken);
    Task<UpdateResult> UpdateAsync(Protos.Therapist therapist, CancellationToken cancellationToken);
    Task<DeleteResult> DeleteAsync(int id, CancellationToken cancellationToken);

    IAsyncEnumerable<Protos.Therapist> StreamAllAsync(int batchSize,
        TherapistFilter filter,
        CancellationToken cancellationToken);
}

internal class TherapistRepository(
    IOptions<ConnectionStringsSettings> connectionStringsSettings,
    IIntFilterOptionsMapper<Int32Filter> intFilterOptionsMapper,
    IStringFilterOptionsMapper<StringFilter> stringFilterOptionsMapper,
    ISqlFilterBuilder sqlFilterBuilder) : ITherapistRepository
{
    private readonly string _connectionString = connectionStringsSettings.Value.CompanyDatabase;

    public async Task<GetResult<Protos.Therapist>> GetAsync(int id, CancellationToken cancellationToken)
    {
        const string sql = """
                           select id as Id, name as Name
                           from Therapist 
                                    where Id = @id
                           """;
        var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);
        await using var connection = new NpgsqlConnection(_connectionString);
        var row = await connection.QuerySingleOrDefaultAsync<TherapistRow>(command);
        return row is null
            ? new GetResult<Protos.Therapist>.NotFound()
            : new GetResult<Protos.Therapist>.Success(row.ToTherapist());
    }

    public async Task<CreateResult> CreateAsync(Protos.Therapist therapist, CancellationToken cancellationToken)
    {
        const string insertSql = """
                                                 insert into Therapist (id, name)
                                                 values (@Id, @Name);
                                 """;
        var command = new CommandDefinition(insertSql, new TherapistRow(therapist), cancellationToken: cancellationToken);
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

    public async Task<UpdateResult> UpdateAsync(Protos.Therapist therapist, CancellationToken cancellationToken)
    {
        const string updateSql = """
                                                 update Therapist set
                                                     name = @Name
                                                 where id = @Id
                                 """;
        var command = new CommandDefinition(updateSql, new TherapistRow(therapist), cancellationToken: cancellationToken);
        await using var connection = new NpgsqlConnection(_connectionString);
        var effectedRows = await connection.ExecuteAsync(command);
        return effectedRows != 1
            ? new UpdateResult.NotFound()
            : new UpdateResult.Success();
    }

    public async Task<DeleteResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        const string sql = "delete from Therapist where Id = @id";
        var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);
        await using var connection = new NpgsqlConnection(_connectionString);
        var effectedRows = await connection.ExecuteAsync(command);
        return effectedRows != 1
            ? new DeleteResult.NotFound()
            : new DeleteResult.Success();
    }

    public IAsyncEnumerable<Protos.Therapist> StreamAllAsync(int batchSize,
        TherapistFilter filter,
        CancellationToken cancellationToken)
        => KeysetPager.StreamAsync<TherapistRow, int, Protos.Therapist>(
            fetchCutoffAsync: FetchCutoffAsync,
            fetchPageAsync: (after, cutoff, fetchBatchSize, fetchCancellationToken) =>
                FetchPageUpToCutoffAsync(after, cutoff, fetchBatchSize, filter, fetchCancellationToken),
            keySelector: row => row.Id,
            map: row => row.ToTherapist(),
            batchSize: batchSize,
            cancellationToken: cancellationToken);

    private async Task<int> FetchCutoffAsync(CancellationToken cancellationToken)
    {
        const string cutoffSql = "select max(Id) from Therapist";
        var command = new CommandDefinition(cutoffSql, cancellationToken: cancellationToken);
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int>(command);
    }

    private async Task<ReadOnlyCollection<TherapistRow>> FetchPageUpToCutoffAsync(
        int? after,
        int cutoff,
        int limit,
        TherapistFilter filter,
        CancellationToken cancellationToken)
    {
        var (whereClause, filterParameters) = sqlFilterBuilder.Build(ToFilterMappings(filter));
        var pageSql = $"""
                       select Id as Id, name as Name
                       from Therapist
                       {whereClause}
                         and (@After is null or Id > @After)
                         and Id <= @Cutoff
                       order by Id
                       limit @Limit
                       """;

        var parameters = new DynamicParameters();
        foreach (var filterParameter in filterParameters)
        {
            parameters.Add(filterParameter.Key, filterParameter.Value);
        }
        parameters.Add("@After", after, DbType.Int32);
        parameters.Add("@Cutoff", cutoff);
        parameters.Add("@Limit", limit);

        var command = new CommandDefinition(
            pageSql,
            parameters,
            cancellationToken: cancellationToken);
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<TherapistRow>(command)
            .ContinueWith(task => task.Result
                    .ToList()
                    .AsReadOnly(),
                cancellationToken);
    }

    private List<SqlFilterBuilder.FilterMapping> ToFilterMappings(TherapistFilter filter)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (filter is null)
            return [];

        var mappings = new List<SqlFilterBuilder.FilterMapping>();

        if (intFilterOptionsMapper.Map(filter.Id) is {} idMapping)
            mappings.Add(new SqlFilterBuilder.FilterMapping("Id", idMapping));

        if (stringFilterOptionsMapper.Map(filter.Name) is { } nameMapping)
            mappings.Add(new SqlFilterBuilder.FilterMapping("name", nameMapping));

        return mappings;
    }
}
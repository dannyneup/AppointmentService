using System.Collections.ObjectModel;
using System.Data;
using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.RepositoryResults;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.SqlFiltering;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.Patient;

internal interface IPatientRepository
{
    Task<Protos.Patient?> GetAsync(string insuranceNumber, CancellationToken cancellationToken);
    Task<CreateResult> CreateAsync(Protos.Patient patient, CancellationToken cancellationToken);
    Task<UpdateResult> UpdateAsync(Protos.Patient patient, CancellationToken cancellationToken);
    Task<DeleteResult> DeleteAsync(string insuranceNumber, CancellationToken cancellationToken);

    IAsyncEnumerable<Protos.Patient> StreamAllAsync(int batchSize,
        PatientFilter filter,
        CancellationToken cancellationToken);
}

internal class PatientRepository(
    IOptions<ConnectionStringsSettings> connectionStringsSettings,
    IIntFilterOptionsMapper<Int32Filter> intFilterOptionsMapper,
    IStringFilterOptionsMapper<StringFilter> stringFilterOptionsMapper,
    ISqlFilterBuilder sqlFilterBuilder) : IPatientRepository
{
    private readonly string _connectionString = connectionStringsSettings.Value.CentralDatabase;

    public async Task<Protos.Patient?> GetAsync(string insuranceNumber, CancellationToken cancellationToken)
    {
        const string sql = """
                           select insurance_number as InsuranceNumber, name as Name, age as Age 
                           from Patient 
                                    where insurance_number = @insuranceNumber
                           """;
        var command = new CommandDefinition(sql, new { insuranceNumber }, cancellationToken: cancellationToken);
        await using var connection = new NpgsqlConnection(_connectionString);
        var row = await connection.QuerySingleOrDefaultAsync<PatientRow>(command);
        return row?.ToPatient();
    }

    public async Task<CreateResult> CreateAsync(Protos.Patient patient, CancellationToken cancellationToken)
    {
        const string insertSql = """
                                                 insert into Patient (insurance_number, name, age)
                                                 values (@InsuranceNumber, @Name, @Age);
                                 """;
        var command = new CommandDefinition(insertSql, new PatientRow(patient), cancellationToken: cancellationToken);
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

    public async Task<UpdateResult> UpdateAsync(Protos.Patient patient, CancellationToken cancellationToken)
    {
        const string updateSql = """
                                                 update Patient set
                                                     name = @Name,
                                                     age = @Age
                                                 where insurance_number = @InsuranceNumber
                                 """;
        var command = new CommandDefinition(updateSql, new PatientRow(patient), cancellationToken: cancellationToken);
        await using var connection = new NpgsqlConnection(_connectionString);
        var effectedRows = await connection.ExecuteAsync(command);
        return effectedRows != 1
            ? new UpdateResult.NotFound()
            : new UpdateResult.Success();
    }

    public async Task<DeleteResult> DeleteAsync(string insuranceNumber, CancellationToken cancellationToken)
    {
        const string sql = "delete from Patient where insurance_number = @insuranceNumber";
        var command = new CommandDefinition(sql, insuranceNumber, cancellationToken: cancellationToken);
        await using var connection = new NpgsqlConnection(_connectionString);
        var effectedRows = await connection.ExecuteAsync(command);
        return effectedRows != 1
            ? new DeleteResult.NotFound()
            : new DeleteResult.Success();
    }

    public IAsyncEnumerable<Protos.Patient> StreamAllAsync(int batchSize,
        PatientFilter filter,
        CancellationToken cancellationToken)
        => KeysetPager.StreamAsync<PatientRow, string, Protos.Patient>(
            fetchCutoffAsync: FetchCutoffAsync,
            fetchPageAsync: (after, cutoff, fetchBatchSize, fetchCancellationToken) =>
                FetchPageUpToCutoffAsync(after, cutoff, fetchBatchSize, filter, fetchCancellationToken),
            keySelector: row => row.InsuranceNumber,
            map: row => row.ToPatient(),
            batchSize: batchSize,
            cancellationToken: cancellationToken);

    private async Task<string?> FetchCutoffAsync(CancellationToken cancellationToken)
    {
        const string cutoffSql = "select max(insurance_number) from Patient";
        var command = new CommandDefinition(cutoffSql, cancellationToken: cancellationToken);
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<string?>(command);
    }

    private async Task<ReadOnlyCollection<PatientRow>> FetchPageUpToCutoffAsync(string? after,
        string cutoff,
        int limit,
        PatientFilter filter,
        CancellationToken cancellationToken)
    {
        var (whereClause, filterParameters) = sqlFilterBuilder.Build(ToFilterMappings(filter));
        var pageSql = $"""
                       select insurance_number as InsuranceNumber, name as Name, age as Age
                       from Patient
                       {whereClause}
                         and (@After is null or insurance_number > @After)
                         and insurance_number <= @Cutoff
                       order by insurance_number
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
        return await connection.QueryAsync<PatientRow>(command)
            .ContinueWith(task => task.Result
                    .ToList()
                    .AsReadOnly(),
                cancellationToken);
    }

    private List<SqlFilterBuilder.FilterMapping> ToFilterMappings(PatientFilter filter)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (filter is null)
            return [];

        var mappings = new List<SqlFilterBuilder.FilterMapping>();

        if (stringFilterOptionsMapper.Map(filter.InsuranceNumber) is {} insuranceNumberMapping)
            mappings.Add(new SqlFilterBuilder.FilterMapping("insurance_number", insuranceNumberMapping));

        if (stringFilterOptionsMapper.Map(filter.Name) is { } nameMapping)
            mappings.Add(new SqlFilterBuilder.FilterMapping("name", nameMapping));

        if(intFilterOptionsMapper.Map(filter.Age) is { } ageMapping)
            mappings.Add(new SqlFilterBuilder.FilterMapping("age", ageMapping));

        return mappings;
    }
}
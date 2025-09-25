using System.Data;
using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Settings;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.SqlFiltering;
using Microsoft.Extensions.Options;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.Patient;

internal class PatientRepository(
    IOptions<ConnectionStringsSettings> connectionStringsSettings,
    IIntFilterOptionsMapper<Int32Filter> intFilterOptionsMapper,
    IStringFilterOptionsMapper<StringFilter> stringFilterOptionsMapper,
    ISqlFilterBuilder sqlFilterBuilder) : PostgresDbRepository<Protos.Patient, PatientRow, string, PatientFilter>(sqlFilterBuilder)
{
    private const string InsuranceNumberColumnName = "insurance_number";
    private const string NameColumnName = "name";
    private const string AgeColumnName = "age";

    protected override string ConnectionString => connectionStringsSettings.Value.CentralDatabase;
    protected override string TableName => "patient";
    protected override string KeyColumnName => InsuranceNumberColumnName;
    protected override DbType KeyColumnDatabaseType => DbType.String;
    protected override Func<PatientRow, string> KeySelector => row => row.InsuranceNumber;
    protected override IReadOnlyList<(string ColumnName, string PropertyName)> ColumnMappings =>
    [
        (InsuranceNumberColumnName, nameof(Protos.Patient.InsuranceNumber)),
        (NameColumnName, nameof(Protos.Patient.Name)),
        (AgeColumnName, nameof(Protos.Patient.Age))
    ];
    protected override Func<PatientRow, Protos.Patient> ToEntity => row => row.ToPatient();
    protected override Func<Protos.Patient, PatientRow> ToRow => entity => new PatientRow(entity);

    protected override Func<PatientFilter?, List<SqlFilterBuilder.FilterMapping>> ToFilterMappings =>
        filter =>
        {
            if (filter is null)
                return [];

            var mappings = new List<SqlFilterBuilder.FilterMapping>();

            if (stringFilterOptionsMapper.Map(filter.InsuranceNumber) is { } insuranceNumberMapping)
                mappings.Add(new SqlFilterBuilder.FilterMapping(InsuranceNumberColumnName, insuranceNumberMapping));

            if (stringFilterOptionsMapper.Map(filter.Name) is { } nameMapping)
                mappings.Add(new SqlFilterBuilder.FilterMapping(NameColumnName, nameMapping));

            if (intFilterOptionsMapper.Map(filter.Age) is { } ageMapping)
                mappings.Add(new SqlFilterBuilder.FilterMapping(AgeColumnName, ageMapping));

            return mappings;
        };
}
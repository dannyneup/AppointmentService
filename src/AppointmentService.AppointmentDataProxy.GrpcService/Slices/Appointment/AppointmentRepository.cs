using System.Data;
using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Repositories;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Settings;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.SqlFiltering;
using Microsoft.Extensions.Options;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.Appointment;

internal class AppointmentRepository(
    IOptions<ConnectionStringsSettings> connectionStringsSettings,
    IFilterOptionsMapper<SqlFilterBuilder.IntFilterOptions, Int32Filter> intFilterOptionsMapper,
    IFilterOptionsMapper<SqlFilterBuilder.StringFilterOptions, StringFilter> stringFilterOptionsMapper,
    IFilterOptionsMapper<SqlFilterBuilder.TimestampFilterOptions, GoogleTimestampFilter> timestampFilterOptionsMapper,
    ISqlFilterBuilder sqlFilterBuilder) : PostgresDbRepository<Protos.Appointment, AppointmentRow, int, AppointmentFilter>(sqlFilterBuilder)
{
    private const string IdColumnName = "id";
    private const string StartColumnName = "start";
    private const string EndColumnName = "end";
    private const string PatientInsuranceNumberColumnName = "patient_insurance_number";
    private const string TherapistIdColumnName = "therapist_id";
    private const string PracticeInstitutionCodeColumnName = "practice_institution_code";
    private const string FixedRemedyDiagnosisCodeColumnName = "fixed_remedy_diagnosis_code";
    private const string IndividualRemedyIdColumnName = "individual_remedy_id";

    protected override string ConnectionString => connectionStringsSettings.Value.CompanyDatabase;
    protected override string TableName => "appointment";
    protected override string KeyColumnName => IdColumnName;
    protected override DbType KeyColumnDatabaseType => DbType.Int32;
    protected override Func<AppointmentRow, int> KeySelector => row => row.Id;
    protected override IReadOnlyList<(string ColumnName, string PropertyName)> ColumnMappings =>
    [
        (IdColumnName, nameof(Protos.Appointment.Id)),
        (StartColumnName, nameof(Protos.Appointment.Start)),
        (EndColumnName, nameof(Protos.Appointment.End)),
        (PatientInsuranceNumberColumnName, nameof(Protos.Appointment.PatientInsuranceNumber)),
        (TherapistIdColumnName, nameof(Protos.Appointment.TherapistId)),
        (PracticeInstitutionCodeColumnName, nameof(Protos.Appointment.PracticeInstitutionCode)),
        (FixedRemedyDiagnosisCodeColumnName, nameof(Protos.Appointment.FixedRemedyDiagnosisCode)),
        (IndividualRemedyIdColumnName, nameof(Protos.Appointment.IndividualRemedyId))
    ];
    protected override Func<AppointmentRow, Protos.Appointment> ToEntity => row => row.ToAppointment();
    protected override Func<Protos.Appointment, AppointmentRow> ToRow => entity => new AppointmentRow(entity);

    protected override Func<AppointmentFilter?, List<SqlFilterBuilder.FilterMapping>> ToFilterMappings =>
        filter =>
        {
            if (filter is null)
                return [];

            var mappings = new List<SqlFilterBuilder.FilterMapping>();

            if (intFilterOptionsMapper.Map(filter.Id) is { } idMapping)
                mappings.Add(new SqlFilterBuilder.FilterMapping(IdColumnName, idMapping));

            if (timestampFilterOptionsMapper.Map(filter.Start) is { } startMapping)
                mappings.Add(new SqlFilterBuilder.FilterMapping(StartColumnName, startMapping));

            if (timestampFilterOptionsMapper.Map(filter.End) is { } endMapping)
                mappings.Add(new SqlFilterBuilder.FilterMapping(EndColumnName, endMapping));

            if (stringFilterOptionsMapper.Map(filter.PatientInsuranceNumber) is { } patientInsuranceNumberMapping)
                mappings.Add(new SqlFilterBuilder.FilterMapping(PatientInsuranceNumberColumnName, patientInsuranceNumberMapping));

            if (intFilterOptionsMapper.Map(filter.TherapistId) is { } therapistIdMapping)
                mappings.Add(new SqlFilterBuilder.FilterMapping(TherapistIdColumnName, therapistIdMapping));

            if (stringFilterOptionsMapper.Map(filter.PracticeInstitutionCode) is { } practiceInstitutionCodeMapping)
                mappings.Add(new SqlFilterBuilder.FilterMapping(PracticeInstitutionCodeColumnName, practiceInstitutionCodeMapping));

            if (stringFilterOptionsMapper.Map(filter.FixedRemedyDiagnosisCode) is { } fixedRemedyDiagnosisCodeMapping)
                mappings.Add(new SqlFilterBuilder.FilterMapping(FixedRemedyDiagnosisCodeColumnName, fixedRemedyDiagnosisCodeMapping));

            if (intFilterOptionsMapper.Map(filter.IndividualRemedyId) is { } individualRemedyIdMapping)
                mappings.Add(new SqlFilterBuilder.FilterMapping(IndividualRemedyIdColumnName, individualRemedyIdMapping));

            return mappings;
        };
}
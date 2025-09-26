using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Settings;
using Microsoft.Extensions.Options;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices;

internal static class AppointmentSlice
{
    public static void MapAppointmentSlice(this WebApplication app)
        => app.MapGrpcService<AppointmentGrpcService>();

    private sealed class AppointmentGrpcService(AppointmentRepository repository)
        : Protos.PatientService.PatientServiceBase
    {
    }

    private class AppointmentRepository(IOptions<ConnectionStringsSettings> connectionStringsSettings)
    {
//         private readonly NpgsqlConnection _connection = new(connectionStringsSettings.Value.CompanyDatabase);
//
//         private sealed class AppointmentRow
//         {
//             public AppointmentRow(
//                 int id,
//                 DateTimeOffset start,
//                 DateTimeOffset end,
//                 string patientInsuranceNumber,
//                 int therapistId,
//                 int practiceInstitutionCode,
//                 int? fixedRemedyId,
//                 int? individualRemedyId
//             )
//             {
//                 Id = id;
//                 Start = start;
//                 End = end;
//                 PatientInsuranceNumber = patientInsuranceNumber;
//                 TherapistId = therapistId;
//                 PracticeInstitutionCode = practiceInstitutionCode;
//                 FixedRemedyId = fixedRemedyId;
//                 IndividualRemedyId = individualRemedyId;
//             }
//
//             public AppointmentRow(Appointment appointment)
//             {
//                 Id = appointment.Id;
//                 Start = appointment.Start.ToDateTimeOffset();
//                 End = appointment.End.ToDateTimeOffset();
//                 PatientInsuranceNumber = appointment.Patient.InsuranceNumber;
//                 TherapistId = appointment.Therapist.Id;
//                 PracticeInstitutionCode = appointment.Practice.InstitutionCode;
//                 FixedRemedyId = appointment.RemedyCase is Appointment.RemedyOneofCase.FixedRemedy
//                     ? appointment.FixedRemedy.DiagnosisCode
//                     : null;
//                 IndividualRemedyId = appointment.RemedyCase is Appointment.RemedyOneofCase.IndividualRemedy
//                     ? appointment.IndividualRemedy.Id
//                     : null;
//             }
//             public int Id { get; }
//             public DateTimeOffset Start { get; }
//             public DateTimeOffset End { get; }
//             public string PatientInsuranceNumber { get; }
//             public int TherapistId { get; }
//             public int PracticeInstitutionCode { get; }
//             public int? FixedRemedyId { get; }
//             public int? IndividualRemedyId { get; }
//
//             public Appointment ToAppointment()
//             {
//                 int? fixedRemedyId = a.FixedRemedyId == 0 ? null : a.FixedRemedyId;
//                 int? individualRemedyId = a.IndividualRemedyId == 0 ? null : a.IndividualRemedyId;
//
//                 return new Appointment
//                 {
//                     Id = Id,
//                     Start = Timestamp.FromDateTimeOffset(Start),
//                     End = Timestamp.FromDateTimeOffset(End),
//                     Patient = PatientInsuranceNumber,
//                     TherapistId = TherapistId,
//                     PracticeId = PracticeInstitutionCode,
//                     FixedRemedyId = FixedRemedyId ?? 0,
//                     IndividualRemedyId = IndividualRemedyId ?? 0
//                 };
//             }
//         }
//
//
//         public async Task<Appointment?> GetAsync(int id)
//         {
//             const string sql = """
//                                select id,
//                                       start_time,
//                                       endtime,
//                                       patient_insurance_number,
//                                       therapist_id,
//                                       practice_id,
//                                       fixed_remedy_id,
//                                       individual_remedy_id
//                                       from appointment left join patient on patient.insurance_number = appointment.patient_insurance_number
//                                       left join therapist on therapist.id = appointment.therapist_id
//                                       left join fixed_remedy on F
//                                       where id = @id
//                                """;
//             var row = await _connection.QuerySingleOrDefaultAsync<AppointmentRow>(sql, id);
//             return row?.ToAppointment();
//         }
//
//         public async Task<bool> CreateAsync(Appointment appointment)
//         {
//             const string insertSql = """
//
//                                                      insert into appointment (id, start_time, endtime, patient_insurance_number, therapist_id, practice_id, fixed_remedy_id, individual_remedy_id)
//                                                      values (@Id, @StartTs, @EndTs, @PatientInsuranceNumber, @TherapistId, @PracticeId, @FixedId, @IndivId)
//                                      """;
//             return await _connection.ExecuteAsync(insertSql, new AppointmentRow(appointment)) == 1;
//         }
//
//         public async Task<bool> UpdateAsync(Appointment appointment)
//         {
//             const string updateSql = """
//
//                                                      update appointment set
//                                                          start_time = @Start,
//                                                          endtime = @End,
//                                                          patient_insurance_number = @PatientInsuranceNumber,
//                                                          therapist_id = @TherapistId,
//                                                          practice_id = @PracticeId,
//                                                          fixed_remedy_id = @FixedRemedyId,
//                                                          individual_remedy_id = @IndividualRemedyId
//                                                      where id = @Id
//                                      """;
//             return await _connection.ExecuteAsync(updateSql, new AppointmentRow(appointment)) == 1;
//         }
//
//         public async Task<bool> DeleteAsync(int id)
//         {
//             const string sql = "delete from appointment where id = @id";
//             return await _connection.ExecuteAsync(sql, id) == 1;
//         }
    }
}
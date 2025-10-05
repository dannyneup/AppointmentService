using Google.Protobuf.WellKnownTypes;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.Appointment;

internal sealed class AppointmentRow(int id, DateTime start, DateTime end, string patientInsuranceNumber, int therapistId, string practiceInstitutionCode, string? fixedRemedyDiagnosisCode, int? individualRemedyId)
{
    public AppointmentRow(Protos.Appointment appointment)
        : this(
            appointment.Id,
            appointment.Start.ToDateTime(),
            appointment.End.ToDateTime(),
            appointment.PatientInsuranceNumber,
            appointment.TherapistId,
            appointment.PracticeInstitutionCode,
            appointment.HasFixedRemedyDiagnosisCode ? appointment.FixedRemedyDiagnosisCode : null,
            appointment.HasIndividualRemedyId ? appointment.IndividualRemedyId : null
        )
    { }

    public int Id { get; } = id;
    public DateTime Start { get; } = start;
    public DateTime End { get; } = end;
    public string PatientInsuranceNumber { get; } = patientInsuranceNumber;
    public int TherapistId { get; } = therapistId;
    public string PracticeInstitutionCode { get; } = practiceInstitutionCode;
    public string? FixedRemedyDiagnosisCode { get; } = fixedRemedyDiagnosisCode;
    public int? IndividualRemedyId { get; } = individualRemedyId;

    public bool HasFixedRemedyDiagnosisCode => !string.IsNullOrWhiteSpace(FixedRemedyDiagnosisCode);
    public bool HasIndividualRemedyId => IndividualRemedyId.HasValue;


    public Protos.Appointment ToAppointment()
    {
        var appointment = new Protos.Appointment
        {
            Id = Id,
            Start = Start.ToTimestamp(),
            End = End.ToTimestamp(),
            PatientInsuranceNumber = PatientInsuranceNumber,
            TherapistId = TherapistId,
            PracticeInstitutionCode = PracticeInstitutionCode,
            FixedRemedyDiagnosisCode = FixedRemedyDiagnosisCode ?? string.Empty,
        };
        if (HasFixedRemedyDiagnosisCode)
            appointment.FixedRemedyDiagnosisCode = FixedRemedyDiagnosisCode;

        if (HasIndividualRemedyId)
            appointment.IndividualRemedyId = IndividualRemedyId!.Value;

        return appointment;
    }
}
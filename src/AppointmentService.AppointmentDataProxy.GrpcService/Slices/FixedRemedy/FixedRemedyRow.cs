namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.FixedRemedy;

internal sealed class FixedRemedyRow(string diagnosisCode, string name)
{
    public FixedRemedyRow(Protos.FixedRemedy therapist) : this(therapist.DiagnosisCode, therapist.Name)
    {
    }
    public string DiagnosisCode { get; } = diagnosisCode;
    public string Name { get; } = name;

    public Protos.FixedRemedy ToFixedRemedy()
        => new()
        {
            DiagnosisCode = DiagnosisCode,
            Name = Name
        };
}
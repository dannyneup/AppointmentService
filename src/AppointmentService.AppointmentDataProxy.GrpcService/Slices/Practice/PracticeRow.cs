namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.Practice;

internal sealed class PracticeRow(string institutionCode, string name)
{
    public PracticeRow(Protos.Practice practice) : this(practice.InstitutionCode, practice.Name)
    {
    }
    public string InstitutionCode { get; } = institutionCode;
    public string Name { get; } = name;

    public Protos.Practice ToPractice()
        => new()
        {
            InstitutionCode = InstitutionCode,
            Name = Name
        };
}
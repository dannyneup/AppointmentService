namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.IndividualRemedy;

internal sealed class IndividualRemedyRow(int id, string name)
{
    public IndividualRemedyRow(Protos.IndividualRemedy therapist) : this(therapist.Id, therapist.Name)
    {
    }
    public int Id { get; } = id;
    public string Name { get; } = name;

    public Protos.IndividualRemedy ToIndividualRemedy()
        => new()
        {
            Id = Id,
            Name = Name
        };
}
namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.Therapist;

internal sealed class TherapistRow(int id, string name)
{
    public TherapistRow(Protos.Therapist therapist) : this(therapist.Id, therapist.Name)
    {
    }
    public int Id { get; } = id;
    public string Name { get; } = name;

    public Protos.Therapist ToTherapist()
        => new()
        {
            Id = Id,
            Name = Name
        };
}
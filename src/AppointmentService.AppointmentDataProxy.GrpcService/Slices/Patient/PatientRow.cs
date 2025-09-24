namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.Patient;

internal sealed class PatientRow(string insuranceNumber, string name, int age)
{
    public PatientRow(Protos.Patient patient) : this(patient.InsuranceNumber, patient.Name, patient.Age)
    {
    }

    public string InsuranceNumber { get; } = insuranceNumber;
    public string Name { get; } = name;
    public int Age { get; } = age;

    public Protos.Patient ToPatient()
        => new()
        {
            InsuranceNumber = InsuranceNumber,
            Name = Name,
            Age = Age
        };
}
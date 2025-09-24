namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.Patient;

internal static partial class PatientSlice
{
    public static IServiceCollection AddPatientSlice(this IServiceCollection services)
        => services
            .AddScoped<IPatientRepository, PatientRepository>();

    public static void MapPatientSlice(this WebApplication app)
        => app.MapGrpcService<PatientGrpcService>();
}
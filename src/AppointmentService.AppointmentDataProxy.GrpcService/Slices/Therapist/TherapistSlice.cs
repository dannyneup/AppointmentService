namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.Therapist;

internal static partial class TherapistSlice
{
    public static IServiceCollection AddTherapistSlice(this IServiceCollection services)
        => services
            .AddScoped<ITherapistRepository, TherapistRepository>();

    public static void MapTherapistSlice(this WebApplication app)
        => app.MapGrpcService<TherapistGrpcService>();
}
using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.Therapist;

internal static partial class TherapistSlice
{
    public static IServiceCollection AddTherapistSlice(this IServiceCollection services)
        => services
            .AddScoped<IRepository<Protos.Therapist, int, TherapistFilter>, TherapistRepository>();

    public static void MapTherapistSlice(this WebApplication app)
        => app.MapGrpcService<TherapistGrpcService>();
}
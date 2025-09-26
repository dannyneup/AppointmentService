using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Repositories;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.FixedRemedy;

internal static class FixedRemedySlice
{
    public static IServiceCollection AddFixedRemedySlice(this IServiceCollection services)
        => services
            .AddScoped<IRepository<Protos.FixedRemedy, string, FixedRemedyFilter>, FixedRemedyRepository>();

    public static void MapFixedRemedySlice(this WebApplication app)
        => app.MapGrpcService<FixedRemedyGrpcService>();
}
using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Repositories;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.Practice;

internal static class PracticeSlice
{
    public static IServiceCollection AddPracticeSlice(this IServiceCollection services)
        => services
            .AddScoped<IRepository<Protos.Practice, string, PracticeFilter>, PracticeRepository>();

    public static void MapPracticeSlice(this WebApplication app)
        => app.MapGrpcService<PracticeGrpcService>();
}
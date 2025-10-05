using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Repositories;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.Appointment;

internal static class AppointmentSlice
{
    public static IServiceCollection AddAppointmentSlice(this IServiceCollection services)
        => services
            .AddScoped<IRepository<Protos.Appointment, int, AppointmentFilter>, AppointmentRepository>();

    public static void MapAppointmentSlice(this WebApplication app)
        => app.MapGrpcService<AppointmentGrpcService>();
}
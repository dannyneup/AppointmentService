using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Repositories;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.Patient;

internal static class PatientSlice
{
    public static IServiceCollection AddPatientSlice(this IServiceCollection services)
        => services
            .AddScoped<IRepository<Protos.Patient, string, PatientFilter>, PatientRepository>();

    public static void MapPatientSlice(this WebApplication app)
        => app.MapGrpcService<PatientGrpcService>();
}
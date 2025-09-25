using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.IndividualRemedy;

internal static partial class IndividualRemedySlice
{
    public static IServiceCollection AddIndividualRemedySlice(this IServiceCollection services)
        => services
            .AddScoped<IRepository<Protos.IndividualRemedy, int, IndividualRemedyFilter>, IndividualRemedyRepository>();

    public static void MapIndividualRemedySlice(this WebApplication app)
        => app.MapGrpcService<IndividualRemedyGrpcService>();
}
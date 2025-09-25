namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.Practice;

internal static partial class PracticeSlice
{
    public static IServiceCollection AddPracticeSlice(this IServiceCollection services)
        => services
            .AddScoped<IPracticeRepository, PracticeRepository>();

    public static void MapPracticeSlice(this WebApplication app)
        => app.MapGrpcService<PracticeGrpcService>();
}
using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.SqlFiltering;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSettings(this IServiceCollection services)
    => services.AddOptions<ConnectionStringsSettings>()
        .BindConfiguration(ConnectionStringsSettings.SectionName)
        .ValidateDataAnnotations()
        .ValidateOnStart()
        .Services;

    public static IServiceCollection AddShared(this IServiceCollection services)
        => services
            .AddScoped<ISqlFilterBuilder, SqlFilterBuilder>()
            .AddScoped<IIntFilterOptionsMapper<Int32Filter>, IntFilterOptionsMapper>()
            .AddScoped<IStringFilterOptionsMapper<StringFilter>, StringFilterOptionsMapper>();
}
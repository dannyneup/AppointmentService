using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Settings;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.SqlFiltering;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSettings(this IServiceCollection services)
        => services
            .AddSettingsOptions<ConnectionStringsSettings>()
            .AddSettingsOptions<StreamingSettings>();

    public static IServiceCollection AddShared(this IServiceCollection services)
        => services
            .AddScoped<ISqlFilterBuilder, SqlFilterBuilder>()
            .AddScoped<IFilterOptionsMapper<SqlFilterBuilder.IntFilterOptions, Int32Filter>, IntFilterOptionsMapper>()
            .AddScoped<IFilterOptionsMapper<SqlFilterBuilder.StringFilterOptions, StringFilter>, StringFilterOptionsMapper>()
            .AddScoped<IFilterOptionsMapper<SqlFilterBuilder.TimestampFilterOptions, GoogleTimestampFilter>, TimestampFilterOptionsMapper>();

    private static IServiceCollection AddSettingsOptions<TSettings>(this IServiceCollection services)
        where TSettings : class, ISettings
        => services.AddOptions<TSettings>()
            .BindConfiguration(TSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart()
            .Services;
}
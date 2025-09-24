using AppointmentService.AppointmentDataProxy.GrpcService.Protos;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared.SqlFiltering;

internal interface IStringFilterOptionsMapper<TTransport>
{
    SqlFilterBuilder.StringFilterOptions? Map(TTransport? source);
}

internal class StringFilterOptionsMapper : IStringFilterOptionsMapper<StringFilter>
{
    public SqlFilterBuilder.StringFilterOptions? Map(StringFilter? source)
        => source is null
            ? null
            : new SqlFilterBuilder.StringFilterOptions(source.Equals_, source.In, source.NotIn, source.Contains, source.StartsWith, source.EndsWith);
}
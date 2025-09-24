using AppointmentService.AppointmentDataProxy.GrpcService.Protos;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared.SqlFiltering;

internal interface IIntFilterOptionsMapper<TTransport>
{
    SqlFilterBuilder.IntFilterOptions? Map(TTransport? source);
}

internal class IntFilterOptionsMapper : IIntFilterOptionsMapper<Int32Filter>
{
    public SqlFilterBuilder.IntFilterOptions? Map(Int32Filter? source)
        => source is null
            ? null
            : new SqlFilterBuilder.IntFilterOptions(
                source.HasEquals_
                    ? source.Equals_
                    : null,
                source.In,
                source.NotIn,
                source.HasMin
                    ? source.Min
                    : null,
                source.HasMax
                    ? source.Max
                    : null);
}
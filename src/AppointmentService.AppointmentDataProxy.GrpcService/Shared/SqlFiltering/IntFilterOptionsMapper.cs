using AppointmentService.AppointmentDataProxy.GrpcService.Protos;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared.SqlFiltering;

internal class IntFilterOptionsMapper : IFilterOptionsMapper<SqlFilterBuilder.IntFilterOptions, Int32Filter>
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
using AppointmentService.AppointmentDataProxy.GrpcService.Protos;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared.SqlFiltering;

internal class
    TimestampFilterOptionsMapper : IFilterOptionsMapper<SqlFilterBuilder.TimestampFilterOptions, GoogleTimestampFilter>
{
    public SqlFilterBuilder.TimestampFilterOptions? Map(GoogleTimestampFilter? source)
        => source is null
            ? null
            : new SqlFilterBuilder.TimestampFilterOptions(
                source.Equals_?.ToDateTimeOffset(),
                source.In?
                    .Select(timestamp => timestamp.ToDateTimeOffset())
                    .ToList(),
                source.NotIn?
                    .Select(timestamp => timestamp.ToDateTimeOffset())
                    .ToList(),
                source.Before?.ToDateTimeOffset(),
                source.After?.ToDateTimeOffset()
                );
}
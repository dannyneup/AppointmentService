using System.Data;
using Dapper;
using NpgsqlTypes;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared;

internal sealed class UtcDateTimeHandler : SqlMapper.TypeHandler<DateTime>
{
    public override void SetValue(IDbDataParameter parameter, DateTime value)
    {
        if (value.Kind != DateTimeKind.Utc)
            value = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        parameter.Value = value;
        if (parameter is Npgsql.NpgsqlParameter npg)
            npg.NpgsqlDbType = NpgsqlDbType.Timestamp; // or TimestampTz if using timestamptz
    }

    public override DateTime Parse(object value)
    {
        var dt = (DateTime)value;
        return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
    }
}
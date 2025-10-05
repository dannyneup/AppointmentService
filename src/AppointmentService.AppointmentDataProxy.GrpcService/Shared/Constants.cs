namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared;

internal static class Constants
{
    internal static class PostgresExceptionSqlStates
    {
        internal const string DuplicateKey = "23505";
        internal const string ForeignKeyViolation = "23503";
    }
}
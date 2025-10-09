namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared;

internal static class Constants
{
    internal static class PostgresExceptionSqlStates
    {
        internal const string DuplicateKey = "23505";
        internal const string ForeignKeyViolation = "23503";
    }

    internal static class Authorization
    {
        internal static class Policies
        {
            internal const string IsGeneral = nameof(IsGeneral);
            internal const string IsCustomer = nameof(IsCustomer);
        }
        internal static class Scopes
        {
            private const string Base = "api:appointment-data/grpc-service";
            internal const string General = $"{Base}/general";
            internal const string Customer = $"{Base}/customer";
        }
    }
}
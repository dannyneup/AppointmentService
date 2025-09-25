namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared.RepositoryResults;

internal abstract record UpdateResult
{
    public sealed record Success : UpdateResult;
    public sealed record NotFound : UpdateResult;
}
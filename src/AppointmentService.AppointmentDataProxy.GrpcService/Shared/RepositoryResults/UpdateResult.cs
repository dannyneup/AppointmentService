namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared.RepositoryResults;

internal record UpdateResult
{
    public sealed record Success : UpdateResult;
    public sealed record NotFound : UpdateResult;
}
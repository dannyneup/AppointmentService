namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared.RepositoryResults;

internal record DeleteResult
{
    public sealed record Success : DeleteResult;
    public sealed record NotFound : DeleteResult;
}
namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared.RepositoryResults;

internal abstract record CreateResult
{
    public sealed record Success : CreateResult;
    public sealed record AlreadyExists : CreateResult;
}
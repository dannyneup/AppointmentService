namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared.RepositoryResults;

internal abstract record DeleteResult
{
    public sealed record Success : DeleteResult;
    public sealed record NotFound : DeleteResult;
}
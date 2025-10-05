namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared.Repositories.Results;

internal abstract record DeleteResult
{
    public sealed record Success : DeleteResult;
    public sealed record NotFound : DeleteResult;
}
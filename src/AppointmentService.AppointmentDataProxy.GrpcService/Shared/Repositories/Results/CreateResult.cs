namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared.Repositories.Results;

internal abstract record CreateResult
{
    public sealed record Success : CreateResult;
    public sealed record AlreadyExists : CreateResult;
    public sealed record ReferenceViolation : CreateResult;
}
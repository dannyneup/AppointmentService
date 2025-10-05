namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared.Repositories.Results;

internal abstract record UpdateResult
{
    public sealed record Success : UpdateResult;

    public sealed record NotFound : UpdateResult;

    internal sealed record ReferenceViolation : UpdateResult;
}
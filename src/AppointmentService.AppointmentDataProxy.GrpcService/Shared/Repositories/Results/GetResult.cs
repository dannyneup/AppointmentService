namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared.Repositories.Results;

internal abstract record GetResult<TEntity>
{
    public sealed record Success(TEntity Entity) : GetResult<TEntity>;
    public sealed record NotFound : GetResult<TEntity>;
}
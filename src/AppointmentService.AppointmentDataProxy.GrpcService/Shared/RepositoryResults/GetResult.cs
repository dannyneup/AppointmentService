namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared.RepositoryResults;

internal abstract record GetResult<TEntity>
{
    public sealed record Success(TEntity Entity) : GetResult<TEntity>;
    public sealed record NotFound : GetResult<TEntity>;
}
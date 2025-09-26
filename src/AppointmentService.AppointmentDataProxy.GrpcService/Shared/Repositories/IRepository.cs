using AppointmentService.AppointmentDataProxy.GrpcService.Shared.RepositoryResults;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared.Repositories;

internal interface IRepository<TEntity, in TIdentifier, in TFilter> where TEntity : class
{
    Task<GetResult<TEntity>> GetAsync(TIdentifier identifier, CancellationToken cancellationToken);
    Task<CreateResult> CreateAsync(TEntity entityToCreate, CancellationToken cancellationToken);
    Task<UpdateResult> UpdateAsync(TEntity entityToUpdate, CancellationToken cancellationToken);
    Task<DeleteResult> DeleteAsync(TIdentifier identifier, CancellationToken cancellationToken);

    IAsyncEnumerable<TEntity> StreamAllAsync(
        int batchSize,
        TFilter filter,
        CancellationToken cancellationToken);
}
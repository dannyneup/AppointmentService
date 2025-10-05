using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Repositories;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Repositories.Results;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Settings;
using Grpc.Core;
using Microsoft.Extensions.Options;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.IndividualRemedy;

internal sealed class IndividualRemedyGrpcService(IRepository<Protos.IndividualRemedy, int, IndividualRemedyFilter> repository, IOptions<StreamingSettings> streamingSettings)
    : IndividualRemedyService.IndividualRemedyServiceBase
{
    public override async Task<CreateIndividualRemedyResponse> Create(CreateIndividualRemedyRequest request, ServerCallContext context)
    {
        var result = await repository.CreateAsync(request.IndividualRemedy, context.CancellationToken);
        return result is CreateResult.AlreadyExists
            ? throw new RpcException(new Status(StatusCode.AlreadyExists, "IndividualRemedy already exists"))
            : new CreateIndividualRemedyResponse();
    }

    public override async Task<GetIndividualRemedyResponse> Get(GetIndividualRemedyRequest request, ServerCallContext context)
    {
        var result = await repository.GetAsync(request.Id, context.CancellationToken);
        return result switch
        {
            GetResult<Protos.IndividualRemedy>.NotFound => throw new RpcException(new Status(StatusCode.NotFound,
                "Individual remedy not found")),
            GetResult<Protos.IndividualRemedy>.Success success => new GetIndividualRemedyResponse { IndividualRemedy = success.Entity },
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public override async Task<UpdateIndividualRemedyResponse> Update(UpdateIndividualRemedyRequest request, ServerCallContext context)
    {
        var result = await repository.UpdateAsync(request.IndividualRemedy, context.CancellationToken);
        return result is UpdateResult.NotFound
            ? throw new RpcException(new Status(StatusCode.NotFound, "Individual remedy not found"))
            : new UpdateIndividualRemedyResponse();
    }

    public override async Task<DeleteIndividualRemedyResponse> Delete(DeleteIndividualRemedyRequest request, ServerCallContext context)
    {
        var result = await repository.DeleteAsync(request.Id, context.CancellationToken);
        return result is DeleteResult.NotFound
            ? throw new RpcException(new Status(StatusCode.NotFound, "Individual remedy not found"))
            : new DeleteIndividualRemedyResponse();
    }

    public override async Task Stream(StreamIndividualRemediesRequest request,
        IServerStreamWriter<Protos.IndividualRemedy> responseStream,
        ServerCallContext context)
    {
        await foreach (var therapist in repository.StreamAllAsync(streamingSettings.Value.BatchSize, request.Filter, context.CancellationToken))
            await responseStream.WriteAsync(therapist);
    }
}
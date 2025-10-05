using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Repositories;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Repositories.Results;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Settings;
using Grpc.Core;
using Microsoft.Extensions.Options;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.FixedRemedy;

internal sealed class FixedRemedyGrpcService(IRepository<Protos.FixedRemedy, string, FixedRemedyFilter> repository, IOptions<StreamingSettings> streamingSettings)
    : FixedRemedyService.FixedRemedyServiceBase
{
    public override async Task<CreateFixedRemedyResponse> Create(CreateFixedRemedyRequest request, ServerCallContext context)
    {
        var result = await repository.CreateAsync(request.FixedRemedy, context.CancellationToken);
        return result is CreateResult.AlreadyExists
            ? throw new RpcException(new Status(StatusCode.AlreadyExists, "Fixed remedy already exists"))
            : new CreateFixedRemedyResponse();
    }

    public override async Task<GetFixedRemedyResponse> Get(GetFixedRemedyRequest request, ServerCallContext context)
    {
        var result = await repository.GetAsync(request.DiagnosisCode, context.CancellationToken);
        return result switch
        {
            GetResult<Protos.FixedRemedy>.NotFound => throw new RpcException(new Status(StatusCode.NotFound,
                "Fixed remedy not found")),
            GetResult<Protos.FixedRemedy>.Success success => new GetFixedRemedyResponse { FixedRemedy = success.Entity },
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public override async Task<UpdateFixedRemedyResponse> Update(UpdateFixedRemedyRequest request, ServerCallContext context)
    {
        var result = await repository.UpdateAsync(request.FixedRemedy, context.CancellationToken);
        return result is UpdateResult.NotFound
            ? throw new RpcException(new Status(StatusCode.NotFound, "Fixed remedy not found"))
            : new UpdateFixedRemedyResponse();
    }

    public override async Task<DeleteFixedRemedyResponse> Delete(DeleteFixedRemedyRequest request, ServerCallContext context)
    {
        var result = await repository.DeleteAsync(request.DiagnosisCode, context.CancellationToken);
        return result is DeleteResult.NotFound
            ? throw new RpcException(new Status(StatusCode.NotFound, "Fixed remedy not found"))
            : new DeleteFixedRemedyResponse();
    }

    public override async Task Stream(StreamFixedRemediesRequest request,
        IServerStreamWriter<Protos.FixedRemedy> responseStream,
        ServerCallContext context)
    {
        await foreach (var fixedRemedy in repository.StreamAllAsync(streamingSettings.Value.BatchSize, request.Filter, context.CancellationToken))
            await responseStream.WriteAsync(fixedRemedy);
    }
}
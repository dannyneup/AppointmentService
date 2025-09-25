using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.RepositoryResults;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Settings;
using Grpc.Core;
using Microsoft.Extensions.Options;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.Practice;

internal sealed class PracticeGrpcService(IPracticeRepository repository, IOptions<StreamingSettings> streamingSettings)
    : PracticeService.PracticeServiceBase
{
    public override async Task<CreatePracticeResponse> Create(CreatePracticeRequest request, ServerCallContext context)
    {
        var result = await repository.CreateAsync(request.Practice, context.CancellationToken);
        return result is CreateResult.AlreadyExists
            ? throw new RpcException(new Status(StatusCode.AlreadyExists, "Practice already exists"))
            : new CreatePracticeResponse();
    }

    public override async Task<GetPracticeResponse> Get(GetPracticeRequest request, ServerCallContext context)
    {
        var result = await repository.GetAsync(request.InstitutionCode, context.CancellationToken);
        return result switch
        {
            GetResult<Protos.Practice>.NotFound => throw new RpcException(new Status(StatusCode.NotFound,
                "Practice not found")),
            GetResult<Protos.Practice>.Success success => new GetPracticeResponse { Practice = success.Entity },
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public override async Task<UpdatePracticeResponse> Update(UpdatePracticeRequest request, ServerCallContext context)
    {
        var result = await repository.UpdateAsync(request.Practice, context.CancellationToken);
        return result is UpdateResult.NotFound
            ? throw new RpcException(new Status(StatusCode.NotFound, "Practice not found"))
            : new UpdatePracticeResponse();
    }

    public override async Task<DeletePracticeResponse> Delete(DeletePracticeRequest request, ServerCallContext context)
    {
        var result = await repository.DeleteAsync(request.InstitutionCode, context.CancellationToken);
        return result is DeleteResult.NotFound
            ? throw new RpcException(new Status(StatusCode.NotFound, "Practice not found"))
            : new DeletePracticeResponse();
    }

    public override async Task Stream(StreamPracticesRequest request,
        IServerStreamWriter<Protos.Practice> responseStream,
        ServerCallContext context)
    {
        await foreach (var practice in repository.StreamAllAsync(streamingSettings.Value.BatchSize, request.Filter, context.CancellationToken))
            await responseStream.WriteAsync(practice);
    }
}
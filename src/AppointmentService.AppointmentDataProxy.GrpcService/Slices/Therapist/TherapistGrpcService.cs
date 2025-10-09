using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Repositories;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Repositories.Results;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Settings;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.Therapist;

[Authorize(Constants.Authorization.Policies.IsCustomer)]
internal sealed class TherapistGrpcService(IRepository<Protos.Therapist, int, TherapistFilter> repository, IOptions<StreamingSettings> streamingSettings)
    : TherapistService.TherapistServiceBase
{
    public override async Task<CreateTherapistResponse> Create(CreateTherapistRequest request, ServerCallContext context)
    {
        var result = await repository.CreateAsync(request.Therapist, context.CancellationToken);
        return result is CreateResult.AlreadyExists
            ? throw new RpcException(new Status(StatusCode.AlreadyExists, "Therapist already exists"))
            : new CreateTherapistResponse();
    }

    public override async Task<GetTherapistResponse> Get(GetTherapistRequest request, ServerCallContext context)
    {
        var result = await repository.GetAsync(request.Id, context.CancellationToken);
        return result switch
        {
            GetResult<Protos.Therapist>.NotFound => throw new RpcException(new Status(StatusCode.NotFound,
                "Therapist not found")),
            GetResult<Protos.Therapist>.Success success => new GetTherapistResponse { Therapist = success.Entity },
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public override async Task<UpdateTherapistResponse> Update(UpdateTherapistRequest request, ServerCallContext context)
    {
        var result = await repository.UpdateAsync(request.Therapist, context.CancellationToken);
        return result is UpdateResult.NotFound
            ? throw new RpcException(new Status(StatusCode.NotFound, "Therapist not found"))
            : new UpdateTherapistResponse();
    }

    public override async Task<DeleteTherapistResponse> Delete(DeleteTherapistRequest request, ServerCallContext context)
    {
        var result = await repository.DeleteAsync(request.Id, context.CancellationToken);
        return result is DeleteResult.NotFound
            ? throw new RpcException(new Status(StatusCode.NotFound, "Therapist not found"))
            : new DeleteTherapistResponse();
    }

    public override async Task Stream(StreamTherapistsRequest request,
        IServerStreamWriter<Protos.Therapist> responseStream,
        ServerCallContext context)
    {
        await foreach (var therapist in repository.StreamAllAsync(streamingSettings.Value.BatchSize, request.Filter, context.CancellationToken))
            await responseStream.WriteAsync(therapist);
    }
}
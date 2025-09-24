using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.RepositoryResults;
using Grpc.Core;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.Patient;

internal sealed class PatientGrpcService(IPatientRepository repository)
    : PatientService.PatientServiceBase
{
    public override async Task<CreatePatientResponse> Create(CreatePatientRequest request, ServerCallContext context)
    {
        var result = await repository.CreateAsync(request.Patient, context.CancellationToken);
        return result is CreateResult.AlreadyExists
            ? throw new RpcException(new Status(StatusCode.AlreadyExists, "Patient already exists"))
            : new CreatePatientResponse();
    }

    public override async Task<GetPatientResponse> Get(GetPatientRequest request, ServerCallContext context)
    {
        var result = await repository.GetAsync(request.InsuranceNumber, context.CancellationToken);
        return new GetPatientResponse
        {
            Patient = result
        };
    }

    public override async Task<UpdatePatientResponse> Update(UpdatePatientRequest request, ServerCallContext context)
    {
        var result = await repository.UpdateAsync(request.Patient, context.CancellationToken);
        return result is UpdateResult.NotFound
            ? throw new RpcException(new Status(StatusCode.NotFound, "Patient not found"))
            : new UpdatePatientResponse();
    }

    public override async Task<DeletePatientResponse> Delete(DeletePatientRequest request, ServerCallContext context)
    {
        var result = await repository.DeleteAsync(request.InsuranceNumber, context.CancellationToken);
        return result is DeleteResult.NotFound
            ? throw new RpcException(new Status(StatusCode.NotFound, "Patient not found"))
            : new DeletePatientResponse();
    }

    public override async Task Stream(StreamPatientsRequest request,
        IServerStreamWriter<Protos.Patient> responseStream,
        ServerCallContext context)
    {
        await foreach (var patient in repository.StreamAllAsync(500, request.Filter, context.CancellationToken))
            await responseStream.WriteAsync(patient);
    }
}
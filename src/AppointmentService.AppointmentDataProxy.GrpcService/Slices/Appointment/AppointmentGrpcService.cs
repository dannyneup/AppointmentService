using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Repositories;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Repositories.Results;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Settings;
using Grpc.Core;
using Microsoft.Extensions.Options;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.Appointment;

internal sealed class AppointmentGrpcService(
    IRepository<Protos.Appointment, int, AppointmentFilter> appointmentRepository,
    IRepository<Protos.Therapist, int, TherapistFilter> therapistRepository,
    IRepository<Protos.IndividualRemedy, int, IndividualRemedyFilter> individualRemedyRepository,
    IRepository<Protos.FixedRemedy, string, FixedRemedyFilter> fixedRemedyRepository,
    IOptions<StreamingSettings> streamingSettings)
    : Protos.AppointmentService.AppointmentServiceBase
{
    public override async Task<CreateAppointmentResponse> Create(CreateAppointmentRequest request,
        ServerCallContext context)
    {
        await EnsureExternalDbConstrainsSatisfied(request.Appointment, context.CancellationToken);

        var result = await appointmentRepository.CreateAsync(request.Appointment, context.CancellationToken);
        return result switch
        {
            CreateResult.AlreadyExists
                => throw new RpcException(new Status(StatusCode.AlreadyExists, "Appointment already exists")),
            CreateResult.ReferenceViolation
                => throw new RpcException(new Status(StatusCode.FailedPrecondition,
                    "Object references non existing entities.")),
            CreateResult.Success
                => new CreateAppointmentResponse(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public override async Task<GetAppointmentResponse> Get(GetAppointmentRequest request, ServerCallContext context)
    {
        var result = await appointmentRepository.GetAsync(request.Id, context.CancellationToken);
        return result switch
        {
            GetResult<Protos.Appointment>.NotFound
                => throw new RpcException(new Status(StatusCode.NotFound, "Appointment not found")),
            GetResult<Protos.Appointment>.Success success
                => new GetAppointmentResponse { Appointment = success.Entity },
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public override async Task<UpdateAppointmentResponse> Update(UpdateAppointmentRequest request,
        ServerCallContext context)
    {
        await EnsureExternalDbConstrainsSatisfied(request.Appointment, context.CancellationToken);
        var result = await appointmentRepository.UpdateAsync(request.Appointment, context.CancellationToken);
        return result switch
        {
            UpdateResult.NotFound
                => throw new RpcException(new Status(StatusCode.NotFound, "Appointment not found")),
            UpdateResult.ReferenceViolation
                => throw new RpcException(new Status(StatusCode.FailedPrecondition,
                    "Object references non existing entities.")),
            UpdateResult.Success
                => new UpdateAppointmentResponse(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public override async Task<DeleteAppointmentResponse> Delete(DeleteAppointmentRequest request,
        ServerCallContext context)
    {
        var result = await appointmentRepository.DeleteAsync(request.Id, context.CancellationToken);
        return result is DeleteResult.NotFound
            ? throw new RpcException(new Status(StatusCode.NotFound, "Appointment not found"))
            : new DeleteAppointmentResponse();
    }

    public override async Task Stream(StreamAppointmentsRequest request,
        IServerStreamWriter<Protos.Appointment> responseStream,
        ServerCallContext context)
    {
        await foreach (var appointment in appointmentRepository.StreamAllAsync(streamingSettings.Value.BatchSize,
                           request.Filter,
                           context.CancellationToken))
            await responseStream.WriteAsync(appointment);
    }

    private async Task<bool> CheckExternalDbConstrainsSatisfiedAsync(Protos.Appointment appointment, CancellationToken cancellationToken)
    {
        if (appointment.HasFixedRemedyDiagnosisCode)
        {
            var result = await fixedRemedyRepository.GetAsync(appointment.FixedRemedyDiagnosisCode, cancellationToken);
            if (result is not GetResult<Protos.FixedRemedy>.Success)
                return false;
        }

        return true;
    }

    private async Task EnsureExternalDbConstrainsSatisfied(Protos.Appointment appointment, CancellationToken cancellationToken)
    {
        var constrainsSatisfied = await CheckExternalDbConstrainsSatisfiedAsync(appointment, cancellationToken);
        if (!constrainsSatisfied)
            throw new RpcException(new Status(StatusCode.FailedPrecondition, "Foreign key contains not satisfied"));
    }
}
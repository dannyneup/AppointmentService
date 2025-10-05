using AppointmentService.AppointmentDataProxy.GrpcService.Shared;
using AppointmentService.AppointmentDataProxy.GrpcService.Slices.Appointment;
using AppointmentService.AppointmentDataProxy.GrpcService.Slices.FixedRemedy;
using AppointmentService.AppointmentDataProxy.GrpcService.Slices.IndividualRemedy;
using AppointmentService.AppointmentDataProxy.GrpcService.Slices.Patient;
using AppointmentService.AppointmentDataProxy.GrpcService.Slices.Practice;
using AppointmentService.AppointmentDataProxy.GrpcService.Slices.Therapist;
using Dapper;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables("AppointmentDataProxy__GrpcService__");

var services = builder.Services;
services.AddGrpc();
services.AddGrpcReflection();

services.AddSettings();
services.AddShared();

services.AddPatientSlice();
services.AddTherapistSlice();
services.AddPracticeSlice();
services.AddFixedRemedySlice();
services.AddIndividualRemedySlice();
services.AddAppointmentSlice();

SqlMapper.AddTypeHandler(new UtcDateTimeHandler());

var app = builder.Build();

app.MapPatientSlice();
app.MapTherapistSlice();
app.MapPracticeSlice();
app.MapFixedRemedySlice();
app.MapIndividualRemedySlice();
app.MapAppointmentSlice();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.Run();

namespace AppointmentService.AppointmentDataProxy.GrpcService
{
    public partial class Program;
}
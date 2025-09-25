
using AppointmentService.AppointmentDataProxy.GrpcService.Shared;
using AppointmentService.AppointmentDataProxy.GrpcService.Slices.FixedRemedy;
using AppointmentService.AppointmentDataProxy.GrpcService.Slices.Patient;
using AppointmentService.AppointmentDataProxy.GrpcService.Slices.Practice;
using AppointmentService.AppointmentDataProxy.GrpcService.Slices.Therapist;

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

var app = builder.Build();

app.MapPatientSlice();
app.MapTherapistSlice();
app.MapPracticeSlice();
app.MapFixedRemedySlice();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.Run();

public partial class Program;
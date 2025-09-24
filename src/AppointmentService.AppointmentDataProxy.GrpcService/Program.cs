
using AppointmentService.AppointmentDataProxy.GrpcService.Shared;
using AppointmentService.AppointmentDataProxy.GrpcService.Slices.Patient;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables("AppointmentDataProxy__GrpcService__");

var services = builder.Services;
services.AddGrpc();
services.AddGrpcReflection();

services.AddSettings();
services.AddShared();
services.AddPatientSlice();

var app = builder.Build();
app.MapPatientSlice();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.Run();

public partial class Program;
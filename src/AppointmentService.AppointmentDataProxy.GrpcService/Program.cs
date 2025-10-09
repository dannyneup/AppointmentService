using AppointmentService.AppointmentDataProxy.GrpcService.Shared;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Authorization;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Settings;
using AppointmentService.AppointmentDataProxy.GrpcService.Slices.Appointment;
using AppointmentService.AppointmentDataProxy.GrpcService.Slices.FixedRemedy;
using AppointmentService.AppointmentDataProxy.GrpcService.Slices.IndividualRemedy;
using AppointmentService.AppointmentDataProxy.GrpcService.Slices.Patient;
using AppointmentService.AppointmentDataProxy.GrpcService.Slices.Practice;
using AppointmentService.AppointmentDataProxy.GrpcService.Slices.Therapist;
using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables("AppointmentDataProxy__GrpcService__");

var services = builder.Services;
services.AddSettings();
services.AddGrpc();
services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var configuration = builder.Configuration
            .GetSection(AuthenticationSettings.SectionName)
            .Get<AuthenticationSettings>();

        options.Authority = configuration?.Authority;
        options.Audience = configuration?.Audience;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidAudience = configuration?.Audience,
            ValidIssuer = configuration?.Authority
        };
        options.RequireHttpsMetadata = false;
    });
services.AddAuthorization(options =>
{
    options.AddPolicy(Constants.Authorization.Policies.IsGeneral,
        policy => policy.RequireScope(Constants.Authorization.Scopes.General));
    options.AddPolicy(Constants.Authorization.Policies.IsCustomer,
        policy => policy.RequireScope(Constants.Authorization.Scopes.Customer));
    options.FallbackPolicy = options.GetPolicy(Constants.Authorization.Policies.IsCustomer);
});
services.AddGrpcReflection();

services.AddShared();

services.AddPatientSlice();
services.AddTherapistSlice();
services.AddPracticeSlice();
services.AddFixedRemedySlice();
services.AddIndividualRemedySlice();
services.AddAppointmentSlice();

SqlMapper.AddTypeHandler(new UtcDateTimeHandler());

var app = builder.Build();

app
    .MapGrpcReflectionService()
    .AllowAnonymous();

app.UseAuthentication();
app.UseAuthorization();

app.MapPatientSlice();
app.MapTherapistSlice();
app.MapPracticeSlice();
app.MapFixedRemedySlice();
app.MapIndividualRemedySlice();
app.MapAppointmentSlice();

app.Run();

namespace AppointmentService.AppointmentDataProxy.GrpcService
{
    public partial class Program;
}
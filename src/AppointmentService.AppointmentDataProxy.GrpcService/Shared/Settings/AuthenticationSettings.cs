using System.ComponentModel.DataAnnotations;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared.Settings;

internal sealed class AuthenticationSettings : ISettings
{
    public static string SectionName => "Authentication";

    [Required(AllowEmptyStrings = false)]
    public required string Authority { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string Audience { get; init; }
}
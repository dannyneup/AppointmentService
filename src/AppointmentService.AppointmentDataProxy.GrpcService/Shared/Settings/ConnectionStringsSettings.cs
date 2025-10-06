using System.ComponentModel.DataAnnotations;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared.Settings;

internal sealed class ConnectionStringsSettings : ISettings
{
    public static string SectionName => "ConnectionStrings";

    [Required(AllowEmptyStrings = false)]
    public string CentralDatabase { get; init; }

    [Required(AllowEmptyStrings = false)]
    public string CompanyDatabase { get; init; }

}
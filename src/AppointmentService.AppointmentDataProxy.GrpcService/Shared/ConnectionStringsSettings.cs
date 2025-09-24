using System.ComponentModel.DataAnnotations;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared;

internal class ConnectionStringsSettings
{
    internal static string SectionName = "ConnectionStrings";

    [Required(AllowEmptyStrings = false)]
    internal string CentralDatabase { get; init; }

    [Required(AllowEmptyStrings = false)]
    internal string CompanyDatabase { get; init; }
}
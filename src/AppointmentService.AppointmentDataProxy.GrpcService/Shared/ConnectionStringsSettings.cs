using System.ComponentModel.DataAnnotations;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared;

internal class ConnectionStringsSettings
{
    [Required(AllowEmptyStrings = false)]
    public string CentralDatabase { get; init; }

    [Required(AllowEmptyStrings = false)]
    public string CompanyDatabase { get; init; }
}
using System.ComponentModel.DataAnnotations;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared.Settings;

internal sealed class StreamingSettings : ISettings
{
    public static string SectionName => "StreamingSettings";

    [Required(AllowEmptyStrings = false)]
    internal int BatchSize { get; init; }

}
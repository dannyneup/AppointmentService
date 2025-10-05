namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared.SqlFiltering;

internal interface IFilterOptionsMapper<out TOut, in TSource>
{
    TOut? Map(TSource? source);
}
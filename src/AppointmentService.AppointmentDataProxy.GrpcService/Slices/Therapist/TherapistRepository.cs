using System.Data;
using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Settings;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.SqlFiltering;
using Microsoft.Extensions.Options;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.Therapist;

internal class TherapistRepository(
    IOptions<ConnectionStringsSettings> connectionStringsSettings,
    IIntFilterOptionsMapper<Int32Filter> intFilterOptionsMapper,
    IStringFilterOptionsMapper<StringFilter> stringFilterOptionsMapper,
    ISqlFilterBuilder sqlFilterBuilder) : PostgresDbRepository<Protos.Therapist, TherapistRow, int, TherapistFilter>(sqlFilterBuilder)
{
    private const string IdColumnName = "id";
    private const string NameColumnName = "name";
    
    protected override string ConnectionString => connectionStringsSettings.Value.CompanyDatabase;
    protected override string TableName => "therapist";
    protected override string KeyColumnName => IdColumnName;
    protected override DbType KeyColumnDatabaseType => DbType.Int32;
    protected override Func<TherapistRow, int> KeySelector => row => row.Id;
    protected override IReadOnlyList<(string ColumnName, string PropertyName)> ColumnMappings =>
    [
        (IdColumnName, nameof(Protos.Therapist.Id)),
        (NameColumnName, nameof(Protos.Therapist.Name)),
    ];
    protected override Func<TherapistRow, Protos.Therapist> ToEntity => row => row.ToTherapist();
    protected override Func<Protos.Therapist, TherapistRow> ToRow => entity => new TherapistRow(entity);

    protected override Func<TherapistFilter?, List<SqlFilterBuilder.FilterMapping>> ToFilterMappings =>
        filter =>
        {
            if (filter is null)
                return [];

            var mappings = new List<SqlFilterBuilder.FilterMapping>();

            if (intFilterOptionsMapper.Map(filter.Id) is { } idMapping)
                mappings.Add(new SqlFilterBuilder.FilterMapping(IdColumnName, idMapping));

            if (stringFilterOptionsMapper.Map(filter.Name) is { } nameMapping)
                mappings.Add(new SqlFilterBuilder.FilterMapping(NameColumnName, nameMapping));

            return mappings;
        };
}
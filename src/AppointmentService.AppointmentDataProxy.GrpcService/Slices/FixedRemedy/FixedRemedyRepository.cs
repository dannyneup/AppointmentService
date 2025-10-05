using System.Data;
using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Repositories;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Settings;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.SqlFiltering;
using Microsoft.Extensions.Options;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.FixedRemedy;

internal class FixedRemedyRepository(
    IOptions<ConnectionStringsSettings> connectionStringsSettings,
    IFilterOptionsMapper<SqlFilterBuilder.StringFilterOptions, StringFilter> stringFilterOptionsMapper,
    ISqlFilterBuilder sqlFilterBuilder) : PostgresDbRepository<Protos.FixedRemedy, FixedRemedyRow, string, FixedRemedyFilter>(sqlFilterBuilder)
{
    protected override string ConnectionString => connectionStringsSettings.Value.CentralDatabase;
    protected override string TableName => "fixed_remedy";
    protected override string KeyColumnName => "diagnosis_code";
    protected override DbType KeyColumnDatabaseType => DbType.String;
    protected override Func<FixedRemedyRow, string> KeySelector => row => row.DiagnosisCode;

    protected override IReadOnlyList<(string ColumnName, string PropertyName)> ColumnMappings =>
    [
        ("diagnosis_code", nameof(Protos.FixedRemedy.DiagnosisCode)),
        ("name", nameof(Protos.FixedRemedy.Name))
    ];
    protected override Func<FixedRemedyRow, Protos.FixedRemedy> ToEntity => row => row.ToFixedRemedy();
    protected override Func<Protos.FixedRemedy, FixedRemedyRow> ToRow => entity => new FixedRemedyRow(entity);

    protected override Func<FixedRemedyFilter?, List<SqlFilterBuilder.FilterMapping>> ToFilterMappings
        => filter
            =>
        {
            if (filter is null)
                return [];

            var mappings = new List<SqlFilterBuilder.FilterMapping>();

            if (stringFilterOptionsMapper.Map(filter.DiagnosisCode) is { } idMapping)
                mappings.Add(new SqlFilterBuilder.FilterMapping("diagnosis_code", idMapping));

            if (stringFilterOptionsMapper.Map(filter.Name) is { } nameMapping)
                mappings.Add(new SqlFilterBuilder.FilterMapping("name", nameMapping));

            return mappings;
        };
}
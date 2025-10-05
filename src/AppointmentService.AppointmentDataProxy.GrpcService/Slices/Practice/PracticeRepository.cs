using System.Data;
using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Repositories;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Settings;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.SqlFiltering;
using Microsoft.Extensions.Options;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.Practice;

internal class PracticeRepository(
    IOptions<ConnectionStringsSettings> connectionStringsSettings,
    IFilterOptionsMapper<SqlFilterBuilder.StringFilterOptions, StringFilter> stringFilterOptionsMapper,
    ISqlFilterBuilder sqlFilterBuilder) : PostgresDbRepository<Protos.Practice, PracticeRow, string, PracticeFilter>(sqlFilterBuilder)
{
    private const string InstitutionCodeColumnName = "institution_code";
    private const string NameColumnName = "name";
    
    protected override string ConnectionString => connectionStringsSettings.Value.CentralDatabase;
    protected override string TableName => "practice";
    protected override string KeyColumnName => InstitutionCodeColumnName;
    protected override DbType KeyColumnDatabaseType => DbType.String;
    protected override Func<PracticeRow, string> KeySelector => row => row.InstitutionCode;
    protected override IReadOnlyList<(string ColumnName, string PropertyName)> ColumnMappings =>
    [
        (InstitutionCodeColumnName, nameof(Protos.Practice.InstitutionCode)),
        (NameColumnName, nameof(Protos.Practice.Name)),
    ];
    protected override Func<PracticeRow, Protos.Practice> ToEntity => row => row.ToPractice();
    protected override Func<Protos.Practice, PracticeRow> ToRow => entity => new PracticeRow(entity);

    protected override Func<PracticeFilter?, List<SqlFilterBuilder.FilterMapping>> ToFilterMappings =>
        filter =>
        {
            if (filter is null)
                return [];

            var mappings = new List<SqlFilterBuilder.FilterMapping>();

            if (stringFilterOptionsMapper.Map(filter.InstitutionCode) is { } insuranceNumberMapping)
                mappings.Add(new SqlFilterBuilder.FilterMapping(InstitutionCodeColumnName, insuranceNumberMapping));

            if (stringFilterOptionsMapper.Map(filter.Name) is { } nameMapping)
                mappings.Add(new SqlFilterBuilder.FilterMapping(NameColumnName, nameMapping));

            return mappings;
        };
}
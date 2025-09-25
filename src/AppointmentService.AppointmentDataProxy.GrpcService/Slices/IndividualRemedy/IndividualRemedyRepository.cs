using System.Data;
using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Settings;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.SqlFiltering;
using Microsoft.Extensions.Options;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Slices.IndividualRemedy;

internal class IndividualRemedyRepository(
    IOptions<ConnectionStringsSettings> connectionStringsSettings,
    IIntFilterOptionsMapper<Int32Filter> intFilterOptionsMapper,
    IStringFilterOptionsMapper<StringFilter> stringFilterOptionsMapper,
    ISqlFilterBuilder sqlFilterBuilder) : PostgresDbRepository<Protos.IndividualRemedy, IndividualRemedyRow, int, IndividualRemedyFilter>(sqlFilterBuilder)
{
    private const string IdColumnName = "id";
    private const string NameColumnName = "name";
    
    protected override string ConnectionString => connectionStringsSettings.Value.CompanyDatabase;
    protected override string TableName => "individual_remedy";
    protected override string KeyColumnName => IdColumnName;
    protected override DbType KeyColumnDatabaseType => DbType.Int32;
    protected override Func<IndividualRemedyRow, int> KeySelector => row => row.Id;
    protected override IReadOnlyList<(string ColumnName, string PropertyName)> ColumnMappings =>
    [
        (IdColumnName, nameof(Protos.IndividualRemedy.Id)),
        (NameColumnName, nameof(Protos.IndividualRemedy.Name)),
    ];
    protected override Func<IndividualRemedyRow, Protos.IndividualRemedy> ToEntity => row => row.ToIndividualRemedy();
    protected override Func<Protos.IndividualRemedy, IndividualRemedyRow> ToRow => entity => new IndividualRemedyRow(entity);

    protected override Func<IndividualRemedyFilter?, List<SqlFilterBuilder.FilterMapping>> ToFilterMappings =>
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
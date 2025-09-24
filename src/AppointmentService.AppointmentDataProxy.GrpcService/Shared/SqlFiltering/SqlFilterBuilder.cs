namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared.SqlFiltering;

internal interface ISqlFilterBuilder
{
    (string whereClause, Dictionary<string, object?> parameters) Build(IEnumerable<SqlFilterBuilder.FilterMapping> filterMappings);
}

internal class SqlFilterBuilder : ISqlFilterBuilder
{
    public interface IFilterOptions;

    public sealed record StringFilterOptions(
        string? IsEqual = null,
        IReadOnlyList<string>? In = null,
        IReadOnlyList<string>? NotIn = null,
        string? Contains = null,
        string? StartsWith = null,
        string? EndsWith = null,
        bool CaseInsensitive = false) : IFilterOptions;

    public sealed record IntFilterOptions(
        int? IsEqual = null,
        IReadOnlyList<int>? In = null,
        IReadOnlyList<int>? NotIn = null,
        int? Min = null,
        int? Max = null) : IFilterOptions;

    public sealed record FilterMapping(
        string ColumnName,
        IFilterOptions FilterOptions
    );


    public (string whereClause, Dictionary<string, object?> parameters) Build(IEnumerable<FilterMapping> filterMappings)
    {
        var where = "where true";
        var parameters = new Dictionary<string, object?>();

        foreach (var filterMapping in filterMappings)
        {
            switch (filterMapping.FilterOptions)
            {
                case StringFilterOptions stringFilterOptions:
                    AppendStringOps(ref where, parameters, filterMapping.ColumnName, stringFilterOptions);
                    break;
                case IntFilterOptions intFilterOptions:
                    AppendInt32Ops(ref where, parameters, filterMapping.ColumnName, intFilterOptions);
                    break;
            }
        }

        return (where, parameters);
    }

    private static void AppendStringOps(
        ref string where,
        Dictionary<string, object?> parameters,
        string column,
        StringFilterOptions filter)
    {
        var caseInsensitive = filter.CaseInsensitive;

        if (!string.IsNullOrEmpty(filter.IsEqual))
        {
            where += $" and {ColumnExpression(column)} = @{column}_eq";
            parameters[$"{column}_eq"] = caseInsensitive
                ? filter.IsEqual.ToLowerInvariant()
                : filter.IsEqual;
        }

        if (filter.In is { Count: > 0 })
        {
            where += $" and {ColumnExpression(column)} = any(@{column}_in)";
            parameters[$"{column}_in"] = caseInsensitive
                ? filter.In.Select(s => s.ToLowerInvariant())
                    .ToArray()
                : filter.In.ToArray();
        }

        if (filter.NotIn is { Count: > 0 })
        {
            where += $" and not ({ColumnExpression(column)} = any(@{column}_notin))";
            parameters[$"{column}_notin"] = caseInsensitive
                ? filter.NotIn.Select(value => value.ToLowerInvariant())
                    .ToArray()
                : filter.NotIn.ToArray();
        }

        if (!string.IsNullOrEmpty(filter.Contains))
        {
            where += $" and {ColumnExpression(column)} like @{column}_contains";
            parameters[$"{column}_contains"] = $"%{(
                caseInsensitive
                    ? filter.Contains.ToLowerInvariant()
                    : filter.Contains
            )}%";

        }

        if (!string.IsNullOrEmpty(filter.StartsWith))
        {
            where += $" and {ColumnExpression(column)} like @{column}_starts";
            parameters[$"{column}_starts"] =
                $"{(
                    caseInsensitive
                        ? filter.StartsWith.ToLowerInvariant()
                        : filter.StartsWith
                )}%";
        }

        if (!string.IsNullOrEmpty(filter.EndsWith))
        {
            where += $" and {ColumnExpression(column)} like @{column}_ends";
            parameters[$"{column}_ends"] =
                $"%{(
                    caseInsensitive
                        ? filter.EndsWith.ToLowerInvariant()
                        : filter.EndsWith
                )}";
        }

        return;

        string ColumnExpression(string c) => caseInsensitive ? $"lower({c})" : c;
    }

    private static void AppendInt32Ops(ref string where,
        Dictionary<string, object?> parameters,
        string column,
        IntFilterOptions filter)
    {
        if (filter.IsEqual.HasValue)
        {
            where += $" and {column} = @{column}_eq";
            parameters[$"{column}_eq"] = filter.IsEqual.Value;
        }

        if (filter.In is { Count: > 0 })
        {
            where += $" and {column} = any(@{column}_in)";
            parameters[$"{column}_in"] = filter.In.ToArray();
        }

        if (filter.NotIn is { Count: > 0 })
        {
            where += $" and not ({column} = any(@{column}_notin))";
            parameters[$"{column}_notin"] = filter.NotIn.ToArray();
        }

        if (filter.Min.HasValue)
        {
            where += $" and {column} >= @{column}_min";
            parameters[$"{column}_min"] = filter.Min.Value;
        }

        if (filter.Max.HasValue)
        {
            where += $" and {column} <= @{column}_max";
            parameters[$"{column}_max"] = filter.Max.Value;
        }
    }
}
using System.Data.Common;

namespace Server.Repository;

public class Filter
{
    private readonly List<string> _conditions = new();
    private readonly List<(string Name, object Value)> _parameters = new();
    private int _paramIndex = 0;

    private string NextParam() => $"@p{_paramIndex++}";

    public void AddFilter(string column, object value)
    {
        if (!string.IsNullOrEmpty(column) && value != null)
        {
            var p = NextParam();
            _conditions.Add($"{column} = {p}");
            _parameters.Add((p, value));
        }
    }

    public void AddLikeFilter(string column, string value)
    {
        if (!string.IsNullOrEmpty(column) && !string.IsNullOrWhiteSpace(value))
        {
            var p = NextParam();
            _conditions.Add($"{column} LIKE {p}");
            _parameters.Add((p, $"%{value}%"));
        }
    }

    public void AddRangeFilter(string column, object start, object end)
    {
        if (!string.IsNullOrEmpty(column) && start != null && end != null)
        {
            var p1 = NextParam();
            var p2 = NextParam();
            _conditions.Add($"{column} BETWEEN {p1} AND {p2}");
            _parameters.Add((p1, start));
            _parameters.Add((p2, end));
        }
    }

    public string BuildWhere() =>
        _conditions.Any() ? " WHERE " + string.Join(" AND ", _conditions) : string.Empty;

    public void ApplyParameters(DbCommand command)
    {
        foreach (var (name, value) in _parameters)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }
    }

    public int Size => _conditions.Count;
    public bool IsEmpty => !_conditions.Any();
}
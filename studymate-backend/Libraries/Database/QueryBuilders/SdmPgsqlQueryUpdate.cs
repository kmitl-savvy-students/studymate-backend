namespace studymate_backend.Libraries.Database.QueryBuilders;

public class SdmPgsqlQueryUpdate(
    string tableName
) : ISdmPgsqlQueryBase
{
    private readonly List<string> _setConditions = [];
    private readonly List<string> _whereConditions = [];
    private string _whereRawQuery = string.Empty;

    public string TableName { get; } = tableName;

    public string Build()
    {
        if (_setConditions.Count == 0)
            throw new InvalidOperationException("No columns specified for update.");

        var command = $"UPDATE \"{TableName}\" SET " + string.Join(", ", _setConditions);

        if (_whereRawQuery != string.Empty)
            command += " " + _whereRawQuery;
        else if (_whereConditions.Count > 0)
            command += " WHERE " + string.Join(" AND ", _whereConditions);

        return command;
    }

    public void Set(string field, string? value)
    {
        var condition = $"\"{field}\" = " + (value == null ? "NULL" : $"'{value.Replace("'", "''")}'");
        _setConditions.Add(condition);
    }

    public void WhereRaw(string rawQuery)
    {
        _whereRawQuery = rawQuery;
    }
    public void WhereEqual(string field, string value)
    {
        var condition = $"\"{field}\" = '{value.Replace("'", "''")}'";
        _whereConditions.Add(condition);
    }
}

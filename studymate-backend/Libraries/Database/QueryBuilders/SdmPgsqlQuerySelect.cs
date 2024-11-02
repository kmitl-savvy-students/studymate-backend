namespace studymate_backend.Libraries.Database.QueryBuilders;

public class SdmPgsqlQuerySelect(
    string tableName
) : ISdmPgsqlQueryBase
{
    private readonly List<string> _whereConditions = [];
    private string _whereRawQuery = string.Empty;

    public string TableName { get; } = tableName;

    public string Build()
    {
        var command = $"SELECT * FROM \"{TableName}\"";

        if (_whereRawQuery != string.Empty)
            command += " " + _whereRawQuery;
        else if (_whereConditions.Count > 0)
            command += " WHERE " + string.Join(" AND ", _whereConditions);

        return command;
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

namespace studymate_backend.Libraries.Database.QueryBuilders;

public class SdmMysqlQueryDelete(
    string tableName
) : ISdmMysqlQueryBase
{
    private readonly List<string> _whereConditions = [];
    private string _whereRawQuery = string.Empty;

    public string TableName { get; } = tableName;

    public string Build()
    {
        var command = $"DELETE FROM `{TableName}`";

        if (!string.IsNullOrEmpty(_whereRawQuery))
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
        var condition = $"`{field}` = '{value.Replace("'", "''")}'";
        _whereConditions.Add(condition);
    }
}
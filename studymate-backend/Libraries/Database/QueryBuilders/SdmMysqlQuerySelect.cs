namespace studymate_backend.Libraries.Database.QueryBuilders;

public class SdmMysqlQuerySelect(
    string tableName
) : ISdmMysqlQueryBase
{
    private readonly List<string> _whereConditions = [];
    private string _whereRawQuery = string.Empty;
    private List<string> _whereClauses = new List<string>();

    public string TableName { get; } = tableName;

    public string Build()
    {
        var command = $"SELECT * FROM `{TableName}`";

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

    public SdmMysqlQuerySelect AddWhereCondition(string field, string value)
    {
        var condition = $"`{field}` = '{value.Replace("'", "''")}'";
        _whereConditions.Add(condition);
        return this;
    }

    public string GetWhereClause()
    {
        return _whereConditions.Count > 0 ? "WHERE " + string.Join(" AND ", _whereConditions) : "";
    }

    public string GetQuery()
    {
        var query = $"SELECT * FROM `{TableName}`";
        if (_whereConditions.Count > 0)
            query += " WHERE " + string.Join(" AND ", _whereConditions);
        return query;
    }
    
    public SdmMysqlQuerySelect Where(string column, string @operator, object value)
    {
        string formattedValue = value is string ? $"'{value}'" : value.ToString();
        _whereClauses.Add($"{column} {@operator} {formattedValue}");
        return this;
    }
}
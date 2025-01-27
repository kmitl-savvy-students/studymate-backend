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

    // ฟังก์ชันใหม่สำหรับเพิ่มเงื่อนไขใน SQL Query
    public SdmPgsqlQuerySelect AddWhereCondition(string field, string value)
    {
        var condition = $"\"{field}\" = '{value.Replace("'", "''")}'";
        _whereConditions.Add(condition);
        return this; // คืนค่า this เพื่อให้รองรับ Method Chaining
    }

    // ฟังก์ชันสำหรับนำเงื่อนไขทั้งหมดไปใช้ใน Query
    public string GetWhereClause()
    {
        return _whereConditions.Count > 0 ? "WHERE " + string.Join(" AND ", _whereConditions) : "";
    }
    
    public string GetQuery()
    {
        var query = $"SELECT * FROM \"{TableName}\"";
        if (_whereConditions.Count > 0)
            query += " WHERE " + string.Join(" AND ", _whereConditions);
        return query;
    }
    
    
}

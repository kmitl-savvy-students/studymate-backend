namespace studymate_backend.Libraries.Database.QueryBuilders;

public class SdmPgsqlQueryInsert(
    string tableName
) : ISdmPgsqlQueryBase
{
    private readonly List<string> _columns = [];
    private readonly List<string> _values = [];

    public string TableName { get; } = tableName;

    public string Build()
    {
        if (_columns.Count == 0 || _values.Count == 0)
            throw new InvalidOperationException("No columns or values specified for insert.");

        var columns = string.Join(", ", _columns);
        var values = string.Join(", ", _values);

        return $"INSERT INTO \"{TableName}\" ({columns}) VALUES ({values}) RETURNING id;";
    }

    public void Insert(string field, string? value)
    {
        _columns.Add($"\"{field}\"");
        _values.Add(value == null ? "NULL" : $"'{value.Replace("'", "''")}'");
    }
}

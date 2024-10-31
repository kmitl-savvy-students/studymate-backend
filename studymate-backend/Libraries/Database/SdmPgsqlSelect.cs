namespace studymate_backend.Libraries.Database;

public class SdmPgsqlSelect(
    string table
) : ISdmPgsqlQueryBuilder
{
    public string Table { get; set; } = table;
    private readonly List<string> conditions = [];

    public void whereEqual(string field, string value)
    {
        var condition = $"\"{field}\" = '{value.Replace("'", "''")}'";
        conditions.Add(condition);
    }

    public string build()
    {
        var command = $"SELECT * FROM \"{Table}\"";

        if (conditions.Count > 0)
        {
            command += " WHERE " + string.Join(" AND ", conditions);
        }

        return command;
    }
}
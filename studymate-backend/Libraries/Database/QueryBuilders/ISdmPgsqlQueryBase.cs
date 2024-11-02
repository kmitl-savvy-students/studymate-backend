namespace studymate_backend.Libraries.Database.QueryBuilders;

public interface ISdmPgsqlQueryBase
{
    protected string TableName { get; }
    protected string GetTableName()
    {
        return TableName;
    }
    public string Build();
}

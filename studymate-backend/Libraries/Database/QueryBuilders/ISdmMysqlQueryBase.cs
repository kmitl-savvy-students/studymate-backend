namespace studymate_backend.Libraries.Database.QueryBuilders;

public interface ISdmMysqlQueryBase
{
    protected string TableName { get; }
    protected string GetTableName()
    {
        return TableName;
    }
    public string Build();
}
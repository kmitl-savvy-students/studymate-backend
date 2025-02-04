using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public interface ISdmBaseMethod<T> where T : IBaseModel
{
    protected static abstract string TableName { get; }
    protected static abstract List<T> ProcessQuery(ISdmMysqlQueryBase queryBuilder, bool isArray = false);
    public static abstract SdmMysqlQuerySelect GetQueryObj();
}

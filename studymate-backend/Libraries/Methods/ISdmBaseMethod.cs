using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public interface ISdmBaseMethod<T> where T : IBaseModel
{
    protected static abstract string tableName { get; }
    protected static abstract List<T> processQuery(SdmPgsqlQuery query, bool isArray = false);
    protected static abstract SdmPgsqlSelect getSelectObj();
}
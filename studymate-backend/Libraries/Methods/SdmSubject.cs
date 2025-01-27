using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public abstract class SdmSubject : ISdmBaseMethod<Subject>
{
    public static string TableName => "Subject";
    public static SdmMysqlQuerySelect GetQueryObj()
    {
        return new SdmMysqlQuerySelect(TableName);
    }
    public static List<Subject> ProcessQuery(ISdmMysqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmMysqlQuery.Execute(queryBuilder);

        var result = new List<Subject>();
        while (query.Next())
        {
            result.Add(new Subject(
                query.ToInt(0),
                query.ToString(1),
                query.ToString(2),
                query.ToInt(3),
                query.ToString(4)
            ));
            if (!isArray) break;
        }

        query.CleanUp();
        return result;
    }

    public static List<Subject> GetAll()
    {
        var select = GetQueryObj();

        var result = ProcessQuery(select, true);
        return result;
    }
    public static Subject? GetBy(string subjectId)
    {
        var select = GetQueryObj();
        select.WhereEqual("Id", subjectId);

        var result = ProcessQuery(select);
        return result.Count == 0 ? null : result[0];
    }
}
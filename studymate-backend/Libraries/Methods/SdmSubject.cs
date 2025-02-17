using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public abstract class SdmSubject : ISdmBaseMethod<Subject>
{
    private static readonly Dictionary<string, Subject> _cache = new();

    public static string TableName => "subject";
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
                query.ToString(0),
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
    public static Subject? GetBy(string? id)
    {
        if (id == null)
            return null;

        if (_cache.TryGetValue(id, out var value))
            return value;

        var select = GetQueryObj();
        select.WhereEqual("sbj_id", id);

        var result = ProcessQuery(select);
        var subject = result.Count == 0 ? null : result[0];
        if (subject == null)
            return null;
        _cache.Add(id, subject);
        return subject;
    }
}
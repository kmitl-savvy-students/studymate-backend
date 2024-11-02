using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Helper;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public class SdmSubject : ISdmBaseMethod<Subject>
{
    public static string TableName => "subject";

    public static SdmPgsqlQuerySelect GetQueryObj()
    {
        return new SdmPgsqlQuerySelect(TableName);
    }

    public static List<Subject> ProcessQuery(ISdmPgsqlQueryBase queryBuilder, bool isArray = false)
    {
        
        var query = SdmPgsqlQuery.Execute(queryBuilder);
        
        var result = new List<Subject>();

        while (query.Next())
        {
            result.Add(new Subject(
                query.ToString(0),
                query.ToString(1),
                query.ToString(2),
                query.ToInt(3),
                query.ToInt(4),
                query.ToInt(5),
                query.ToString(6),
                query.ToString(7),
                query.ToInt(8),
                query.ToString(9),
                query.ToInt(10),
                query.ToInt(11),
                query.ToString(12),
                query.ToString(13),
                query.ToString(14),
                query.ToString(15),
                query.ToString(16),
                new SdmDateTime(query.ToDateTime(17))
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

    public static Subject? GetById(string subjectId)
    {
        var select = GetQueryObj();
        select.WhereEqual("subject_id", subjectId);

        var result = ProcessQuery(select, true);
        if (result.Count == 0)
            return null;
        return result[0];
    }
}

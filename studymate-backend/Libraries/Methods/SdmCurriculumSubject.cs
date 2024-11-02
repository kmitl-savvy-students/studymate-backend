using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public class SdmCurriculumSubject : ISdmBaseMethod<CurriculumSubject>
{
    public static string TableName => "cu_curri_subject";

    public static SdmPgsqlQuerySelect GetQueryObj()
    {
        return new SdmPgsqlQuerySelect(TableName);
    }

    public static List<CurriculumSubject> ProcessQuery(ISdmPgsqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmPgsqlQuery.Execute(queryBuilder);
        
        var result = new List<CurriculumSubject>();
        
        while (query.Next())
        {
            result.Add(new CurriculumSubject(
                query.ToString(0),
                query.ToInt(1),
                query.ToInt(2),
                query.ToInt(3),
                query.ToString(4),
                query.ToString(5),
                query.ToString(6),
                query.ToString(7)
            ));
            if (!isArray) break;
        }
        
        query.CleanUp();
        return result;
    }

    public static List<CurriculumSubject> GetAll()
    {
        var select = GetQueryObj();

        var result = ProcessQuery(select, true);
        return result;
    }

    public static CurriculumSubject? GetById(string subjectId)
    {
        var select = GetQueryObj();
        select.WhereEqual("subject_id", subjectId);

        var result = ProcessQuery(select, true);
        if (result.Count == 0)
            return null;
        return result[0];
    }
}

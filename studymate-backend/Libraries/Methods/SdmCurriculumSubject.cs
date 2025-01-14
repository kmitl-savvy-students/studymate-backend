using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public abstract class SdmCurriculumSubject : ISdmBaseMethod<CurriculumSubject>
{
    public static string TableName => "curriculum_subject";
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
                SdmSubject.GetBy(query.ToString(0)),
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
    public static List<CurriculumSubject> GetAllBy(string uniqueId, string year)
    {
        var select = GetQueryObj();
        select.WhereEqual("unique_id", uniqueId);
        select.WhereEqual("year", year);

        var result = ProcessQuery(select, true);
        return result;
    }
    public static List<CurriculumSubject> GetAllBy(int categoryId, int groupId, int subgroupId, string uniqueId, string year)
    {
        var select = GetQueryObj();
        select.WhereEqual("category_id", categoryId.ToString());
        select.WhereEqual("group_id", groupId.ToString());
        select.WhereEqual("subgroup_id", subgroupId.ToString());
        select.WhereEqual("unique_id", uniqueId);
        select.WhereEqual("year", year);

        var result = ProcessQuery(select, true);
        return result;
    }

    public static CurriculumSubject? GetBy(string subjectId, string uniqueId, string year)
    {
        var select = GetQueryObj();
        select.WhereEqual("subject_id", subjectId);
        select.WhereEqual("unique_id", uniqueId);
        select.WhereEqual("year", year);

        var result = ProcessQuery(select);
        if (result.Count == 0)
            return null;
        return result[0];
    }
    
    public static List<CurriculumSubject> QueryBy(string uniqueId, string year, string categoryId, string groupId, string subgroupId)
    {
        var select = GetQueryObj();
        select.WhereEqual("unique_id", uniqueId);
        select.WhereEqual("year", year);
        select.WhereEqual("category_id", categoryId);
        select.WhereEqual("group_id", groupId);
        select.WhereEqual("subgroup_id", subgroupId);

        var result = ProcessQuery(select, true);
        return result;
    }
}

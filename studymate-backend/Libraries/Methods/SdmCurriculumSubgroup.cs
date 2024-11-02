using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public class SdmCurriculumSubgroup : ISdmBaseMethod<CurriculumSubgroup>
{
    public static string TableName => "curriculum_subgroup";

    public static SdmPgsqlQuerySelect GetQueryObj()
    {
        return new SdmPgsqlQuerySelect(TableName);
    }

    public static List<CurriculumSubgroup> ProcessQuery(ISdmPgsqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmPgsqlQuery.Execute(queryBuilder);

        var result = new List<CurriculumSubgroup>();

        while (query.Next())
        {
            result.Add(new CurriculumSubgroup(
                query.ToInt(0),
                query.ToInt(1),
                query.ToInt(2),
                query.ToString(3),
                query.ToString(4),
                query.ToString(5),
                query.ToInt(6),
                query.ToInt(7),
                query.ToString(8),
                query.ToString(9)
            ));
            if (!isArray) break;
        }

        query.CleanUp();
        return result;
    }

    public static List<CurriculumSubgroup> GetAll()
    {
        var select = GetQueryObj();

        var result = ProcessQuery(select, true);
        return result;
    }

    public static CurriculumSubgroup? GetBy(int categoryId, int groupId, int subgroupId, string uniqueId, string year)
    {
        var select = GetQueryObj();
        select.WhereEqual("category_id", categoryId.ToString());
        select.WhereEqual("group_id", groupId.ToString());
        select.WhereEqual("subgroup_id", subgroupId.ToString());
        select.WhereEqual("unique_id", uniqueId);
        select.WhereEqual("year", year);

        var result = ProcessQuery(select);
        if (result.Count == 0)
            return null;
        return result[0];
    }
}

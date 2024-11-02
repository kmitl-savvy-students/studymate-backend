using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public class SdmCurriculumSubgroup : ISdmBaseMethod<CurriculumSubgroup>
{
    public static string TableName => "cu_curri_subgroup";

    public static SdmPgsqlQuerySelect GetQueryObj()
    {
        return new SdmPgsqlQuerySelect(TableName);
    }

    public static List<CurriculumSubgroup> ProcessQuery(ISdmPgsqlQueryBase queryBuilder, bool isArray = false)
    {
        // var query = SdmPgsqlQuery.Execute(queryBuilder);
        // var reader = query.GetReader();
        // if (reader == null)
        //     return [];
        //
        // var result = new List<CurriculumSubgroup>();
        //
        // while (reader.Read())
        // {
        //     var curriculumSubgroup = new CurriculumSubgroup(
        //         reader.GetInt32(0),
        //         reader.GetInt32(1),
        //         reader.GetInt32(2),
        //         reader.GetString(3),
        //         reader.GetString(4),
        //         reader.GetString(5),
        //         reader.GetInt32(6),
        //         reader.GetInt32(7),
        //         reader.GetString(8),
        //         reader.GetString(9)
        //     );
        //
        //     result.Add(curriculumSubgroup);
        //
        //     if (!isArray)
        //         return result;
        // }
        //
        // return result;
        return [];
    }

    public static List<CurriculumSubgroup> GetAll()
    {
        var select = GetQueryObj();

        var result = ProcessQuery(select, true);
        return result;
    }

    public static List<CurriculumSubgroup> GetByCatIdAndGroupIdAndSubgroupId(int cCatId, int cGroupId, int cSubgroupId)
    {
        var select = GetQueryObj();
        select.WhereEqual("c_cat_id", cCatId.ToString());
        select.WhereEqual("c_group_id", cGroupId.ToString());
        select.WhereEqual("c_subgroup_id", cSubgroupId.ToString());

        var result = ProcessQuery(select, true);
        if (result.Count == 0)
            return [];
        return result;
    }
}

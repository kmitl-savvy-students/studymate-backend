using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public class SdmCurriculumSubgroup : ISdmBaseMethod<CurriculumSubgroup>
{
    public static string tableName => "cu_curri_subgroup";

    public static SdmPgsqlSelect getSelectObj()
    {
        var builderSelect = new SdmPgsqlSelect(tableName);
        return builderSelect;
    }
    public static List<CurriculumSubgroup> processQuery(SdmPgsqlQuery query, bool isArray = false)
    {
        var result = new List<CurriculumSubgroup>();

        var reader = query.getReader();
        if (reader == null)
            return result;

        while (reader.Read())
        {
            var curriculumSubgroup = new CurriculumSubgroup(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetInt32(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.GetInt32(6),
                reader.GetInt32(7),
                reader.GetString(8),
                reader.GetString(8)
            );

            result.Add(curriculumSubgroup);

            if (!isArray)
                return result;
        }

        return result;
    }

    public static List<CurriculumSubgroup> getAll()
    {
        var select = getSelectObj();

        var result = processQuery(new SdmPgsqlQuery(select), true);
        return result;
    }
    
    public static List<CurriculumSubgroup> getByCatIdAndGroupIdAndSubgroupId(int c_cat_id, int c_group_id, int c_subgroup_id)
    {
        var select = getSelectObj();
        select.whereEqual("c_cat_id", c_cat_id.ToString());
        select.whereEqual("c_group_id", c_group_id.ToString());
        select.whereEqual("c_subgroup_id", c_subgroup_id.ToString());

        var result = processQuery(new SdmPgsqlQuery(select), true);
        if (result.Count == 0)
            return null;
        return result;
    }
    
}
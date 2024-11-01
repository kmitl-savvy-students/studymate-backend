using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public class SdmCurriculumGroup : ISdmBaseMethod<CurriculumGroup>
{
    public static string tableName => "cu_curri_group";

    public static SdmPgsqlSelect getSelectObj()
    {
        var builderSelect = new SdmPgsqlSelect(tableName);
        return builderSelect;
    }
    public static List<CurriculumGroup> processQuery(SdmPgsqlQuery query, bool isArray = false)
    {
        var result = new List<CurriculumGroup>();

        var reader = query.getReader();
        if (reader == null)
            return result;

        while (reader.Read())
        {
            var curriculumGroup = new CurriculumGroup(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetInt32(5),
                reader.GetInt32(6),
                reader.GetString(7),
                reader.GetString(8),
                reader.GetString(9)
            );

            result.Add(curriculumGroup);

            if (!isArray)
                return result;
        }

        return result;
    }

    public static List<CurriculumGroup> getAll()
    {
        var select = getSelectObj();

        var result = processQuery(new SdmPgsqlQuery(select), true);
        return result;
    }

    public static List<CurriculumGroup> getByCatIdAndGroupId(int c_cat_id, int c_group_id)
    {
        var select = getSelectObj();
        select.whereEqual("c_cat_id", c_cat_id.ToString());
        select.whereEqual("c_group_id", c_group_id.ToString());

        var result = processQuery(new SdmPgsqlQuery(select), true);
        if (result.Count == 0)
            return [];
        return result;
    }
}

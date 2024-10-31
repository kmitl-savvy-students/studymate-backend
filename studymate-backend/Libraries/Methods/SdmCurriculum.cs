using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public class SdmCurriculum : ISdmBaseMethod<Curriculum>
{
    public static string tableName => "curriculum";

    public static SdmPgsqlSelect getSelectObj()
    {
        var builderSelect = new SdmPgsqlSelect(tableName);
        return builderSelect;
    }
    public static List<Curriculum> processQuery(SdmPgsqlQuery query, bool isArray = false)
    {
        var result = new List<Curriculum>();

        var reader = query.getReader();
        if (reader == null)
            return result;

        while (reader.Read())
        {
            var curriculum = new Curriculum(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.GetString(6),
                reader.GetString(7),
                reader.GetString(8)
            );

            result.Add(curriculum);

            if (!isArray)
                return result;
        }

        return result;
    }

    public static List<Curriculum> getAll()
    {
        var select = getSelectObj();

        var result = processQuery(new SdmPgsqlQuery(select), true);
        return result;
    }
    public static Curriculum? getById(int id)
    {
        var select = getSelectObj();
        select.whereEqual("id", id.ToString());

        var result = processQuery(new SdmPgsqlQuery(select));
        if (result.Count == 0)
            return null;
        return result[0];
    }
    public static Curriculum? getByUniqueIdYear(string uniqueId, string year)
    {
        var select = getSelectObj();
        select.whereEqual("unique_id", uniqueId);
        select.whereEqual("year", year);

        var result = processQuery(new SdmPgsqlQuery(select));
        if (result.Count == 0)
            return null;
        return result[0];
    }
}

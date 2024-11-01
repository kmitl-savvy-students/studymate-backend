using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public class SdmCurriculumSubject : ISdmBaseMethod<CurriculumSubject>
{
    public static string tableName => "cu_curri_subject";

    public static SdmPgsqlSelect getSelectObj()
    {
        var builderSelect = new SdmPgsqlSelect(tableName);
        return builderSelect;
    }
    public static List<CurriculumSubject> processQuery(SdmPgsqlQuery query, bool isArray = false)
    {
        var result = new List<CurriculumSubject>();

        var reader = query.getReader();
        if (reader == null)
            return result;

        while (reader.Read())
        {
            var curriculumSubject = new CurriculumSubject(
                reader.GetString(0),
                reader.GetInt32(1),
                reader.GetInt32(2),
                reader.GetInt32(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.GetString(6),
                reader.GetString(7)
            );

            result.Add(curriculumSubject);

            if (!isArray)
                return result;
        }

        return result;
    }

    public static List<CurriculumSubject> getAll()
    {
        var select = getSelectObj();

        var result = processQuery(new SdmPgsqlQuery(select), true);
        return result;
    }

    public static CurriculumSubject? getById(string subject_id)
    {
        var select = getSelectObj();
        select.whereEqual("subject_id", subject_id);

        var result = processQuery(new SdmPgsqlQuery(select), true);
        if (result.Count == 0)
            return null;
        return result[0];
    }
}

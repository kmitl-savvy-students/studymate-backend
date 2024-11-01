using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public class SdmSubject : ISdmBaseMethod<Subject>
{
    public static string tableName => "subject";

    public static SdmPgsqlSelect getSelectObj()
    {
        var builderSelect = new SdmPgsqlSelect(tableName);
        return builderSelect;
    }
    public static List<Subject> processQuery(SdmPgsqlQuery query, bool isArray = false)
    {
        var result = new List<Subject>();

        var reader = query.getReader();
        if (reader == null)
            return result;

        while (reader.Read())
        {
            var subject = new Subject(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3),
                reader.GetInt32(4),
                reader.GetInt32(5),
                reader.GetString(6),
                reader.GetString(7),
                reader.GetInt32(8),
                reader.GetString(9),
                reader.GetInt32(10),
                reader.GetInt32(11),
                reader.GetString(12),
                reader.GetString(13),
                reader.GetString(14),
                reader.GetString(15),
                reader.GetString(16),
                reader.GetDateTime(17)
            );

            result.Add(subject);

            if (!isArray)
                return result;
        }

        return result;
    }

    public static List<Subject> getAll()
    {
        var select = getSelectObj();

        var result = processQuery(new SdmPgsqlQuery(select), true);
        return result;
    }

    public static Subject? getById(string subject_id)
    {
        var select = getSelectObj();
        select.whereEqual("subject_id", subject_id);

        var result = processQuery(new SdmPgsqlQuery(select), true);
        if (result.Count == 0)
            return null;
        return result[0];
    }
}

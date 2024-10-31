using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Enums;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public class SdmUser : ISdmBaseMethod<User>
{
    public static string tableName => "user";

    public static SdmPgsqlSelect getSelectObj()
    {
        var select = new SdmPgsqlSelect(tableName);
        return select;
    }
    public static List<User> processQuery(SdmPgsqlQuery query, bool isArray = false)
    {
        var result = new List<User>();

        var reader = query.getReader();
        if (reader == null)
            return result;

        while (reader.Read())
        {
            var user = new User(
                reader.GetString(0),
                reader.GetString(1),
                EnumBase.Get<EnumGender>(reader.GetString(2)) ?? EnumGender.OTHER,
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.GetString(6),
                SdmCurriculum.getById(reader.GetInt32(7))
            );

            result.Add(user);

            if (!isArray)
                return result;
        }

        return result;
    }

    public static List<User> getAll()
    {
        var select = getSelectObj();

        var result = processQuery(new SdmPgsqlQuery(select), true);
        return result;
    }
    public static User? getById(string id)
    {
        var select = getSelectObj();
        select.whereEqual("id", id);

        var result = processQuery(new SdmPgsqlQuery(select));
        if (result.Count == 0)
            return null;
        return result[0];
    }

    public static void insert(User user)
    {
    }
}

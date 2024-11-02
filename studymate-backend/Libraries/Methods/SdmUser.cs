using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Enums;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public class SdmUser : ISdmBaseMethod<User>
{
    public static string TableName => "user";

    public static SdmPgsqlQuerySelect GetQueryObj()
    {
        return new SdmPgsqlQuerySelect(TableName);
    }

    public static List<User> ProcessQuery(ISdmPgsqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmPgsqlQuery.Execute(queryBuilder);

        var result = new List<User>();

        while (query.Next())
        {
            result.Add(new User(
                query.ToString(0),
                query.ToString(1),
                EnumBase.Get<EnumGender>(query.ToString(2)) ?? EnumGender.OTHER,
                query.ToString(3),
                query.ToString(4),
                query.ToString(5),
                query.ToString(6),
                SdmCurriculum.GetById(query.ToInt(7))
            ));
            if (!isArray) break;
        }

        query.CleanUp();
        return result;
    }

    public static List<User> GetAll()
    {
        var select = GetQueryObj();

        var result = ProcessQuery(select, true);
        return result;
    }

    public static User? GetById(string id)
    {
        var select = GetQueryObj();
        select.WhereEqual("id", id);

        var result = ProcessQuery(select);
        if (result.Count == 0)
            return null;
        return result[0];
    }

    public static void Insert(User user)
    {
        var insert = new SdmPgsqlQueryInsert(TableName);

        insert.Insert("id", user.id);
        insert.Insert("password", user.password);
        insert.Insert("gender", user.gender.GetName());
        insert.Insert("name_nick", user.nameNick);
        insert.Insert("name_first", user.nameFirst);
        insert.Insert("name_last", user.nameLast);
        insert.Insert("profile", user.profile);
        insert.Insert("curriculum_id", user.curriculum?.id.ToString());

        SdmPgsqlQuery.Execute(insert);
    }

    public static void Update(User user)
    {
        var update = new SdmPgsqlQueryUpdate(TableName);

        update.Set("name_nick", user.nameNick);
        update.Set("name_first", user.nameNick);
        update.Set("name_last", user.nameNick);
        update.Set("profile", user.nameNick);
        update.Set("curriculum_id", user.curriculum?.id.ToString());

        update.WhereEqual("id", user.id);

        SdmPgsqlQuery.Execute(update);
    }
}

using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public abstract class SdmUser : ISdmBaseMethod<User>
{
    public static string TableName => "User";

    public static SdmMysqlQuerySelect GetQueryObj()
    {
        return new SdmMysqlQuerySelect(TableName);
    }

    public static List<User> ProcessQuery(ISdmMysqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmMysqlQuery.Execute(queryBuilder);

        var result = new List<User>();

        while (query.Next())
        {
            result.Add(new User(
                query.ToString(0),
                query.ToString(1),
                query.ToString(2),
                query.ToString(3),
                query.ToString(4),
                query.ToString(5),
                SdmCurriculum.GetBy(query.ToInt(6))
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

    public static User? GetBy(string id)
    {
        var select = GetQueryObj();
        select.WhereEqual("Id", id);

        var result = ProcessQuery(select);
        return result.Count == 0 ? null : result[0];
    }

    public static void Insert(User user)
    {
        var insert = new SdmMysqlQueryInsert(TableName);

        insert.Insert("Id", user.Id);
        insert.Insert("Password", user.Password);
        insert.Insert("NameNick", user.NameNick);
        insert.Insert("NameFirst", user.NameFirst);
        insert.Insert("NameLast", user.NameLast);
        insert.Insert("Profile", user.Profile);
        insert.Insert("CurriculumId", user.Curriculum?.id.ToString());

        var query = SdmMysqlQuery.Execute(insert);
        query.CleanUp();
    }

    public static void Update(User user)
    {
        var update = new SdmMysqlQueryUpdate(TableName);

        update.Set("NameNick", user.NameNick);
        update.Set("NameFirst", user.NameFirst);
        update.Set("NameLast", user.NameLast);
        update.Set("Profile", user.Profile);
        update.Set("CurriculumId", user.Curriculum?.id.ToString());

        update.WhereEqual("Id", user.Id);

        var query = SdmMysqlQuery.Execute(update);
        query.CleanUp();
    }
}

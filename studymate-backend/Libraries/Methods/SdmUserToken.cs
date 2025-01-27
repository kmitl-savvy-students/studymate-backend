using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Helper;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public abstract class SdmUserToken : ISdmBaseMethod<UserToken>
{
    public static string TableName => "UserToken";
    public static SdmMysqlQuerySelect GetQueryObj()
    {
        return new SdmMysqlQuerySelect(TableName);
    }
    public static List<UserToken> ProcessQuery(ISdmMysqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmMysqlQuery.Execute(queryBuilder);

        var result = new List<UserToken>();
        while (query.Next())
        {
            result.Add(new UserToken(
                query.ToString(0),
                SdmUser.GetBy(query.ToInt(1)),
                new SdmDateTime(query.ToDateTime(2)),
                new SdmDateTime(query.ToDateTime(3))
            ));
            if (!isArray) break;
        }

        query.CleanUp();
        return result;
    }

    public static List<UserToken> GetAll()
    {
        var select = GetQueryObj();

        var result = ProcessQuery(select, true);
        return result;
    }
    public static UserToken? GetBy(string id)
    {
        var select = GetQueryObj();
        select.WhereEqual("Id", id);

        var result = ProcessQuery(select);
        return result.Count == 0 ? null : result[0];
    }
    public static UserToken? GetBy(User user)
    {
        var select = GetQueryObj();
        select.WhereEqual("UserId", user.Id.ToString());

        var result = ProcessQuery(select);
        return result.Count == 0 ? null : result[0];
    }

    public static void Insert(UserToken userToken)
    {
        var insert = new SdmMysqlQueryInsert(TableName);

        insert.Insert("Id", userToken.id);
        insert.Insert("UserId", userToken.user?.Id.ToString());
        insert.Insert("Created", userToken.created.ToString());
        insert.Insert("Expired", userToken.expired.ToString());

        var query = SdmMysqlQuery.Execute(insert);
        query.CleanUp();
    }
    public static void DeleteBy(UserToken userToken)
    {
        var delete = new SdmMysqlQueryDelete(TableName);
        delete.WhereEqual("Id", userToken.id);

        var query = SdmMysqlQuery.Execute(delete);
        query.CleanUp();
    }
}
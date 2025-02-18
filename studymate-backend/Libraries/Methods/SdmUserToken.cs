using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Helper;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public abstract class SdmUserToken : ISdmBaseMethod<UserToken>
{
    public static string TableName => "user_token";
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

    public static UserToken? GetBy(string id)
    {
        var select = GetQueryObj();
        select.WhereEqual("ut_id", id);

        var result = ProcessQuery(select);
        return result.Count == 0 ? null : result[0];
    }
    public static UserToken? GetBy(User user)
    {
        var select = GetQueryObj();
        select.WhereEqual("ut_u_id", user.Id.ToString());

        var result = ProcessQuery(select);
        return result.Count == 0 ? null : result[0];
    }

    public static void Insert(UserToken userToken)
    {
        var insert = new SdmMysqlQueryInsert(TableName);

        insert.Insert("ut_id", userToken.Id);
        insert.Insert("ut_u_id", userToken.User?.Id.ToString());
        insert.Insert("ut_date_created", userToken.DateCreated.ToString());
        insert.Insert("ut_date_expired", userToken.DateExpired.ToString());

        var query = SdmMysqlQuery.Execute(insert);
        query.CleanUp();
    }
    public static void DeleteBy(UserToken userToken)
    {
        var delete = new SdmMysqlQueryDelete(TableName);
        delete.WhereEqual("ut_id", userToken.Id);

        var query = SdmMysqlQuery.Execute(delete);
        query.CleanUp();
    }
}
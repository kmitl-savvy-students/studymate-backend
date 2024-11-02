using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Helper;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public class SdmUserToken : ISdmBaseMethod<UserToken>
{
    public static string TableName => "user_token";

    public static SdmPgsqlQuerySelect GetQueryObj()
    {
        return new SdmPgsqlQuerySelect(TableName);
    }

    public static List<UserToken> ProcessQuery(ISdmPgsqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmPgsqlQuery.Execute(queryBuilder);

        var result = new List<UserToken>();

        while (query.Next())
        {
            result.Add(new UserToken(
                query.ToString(0),
                SdmUser.GetById(query.ToString(1)),
                new SdmDateTime(query.ToString(2)),
                new SdmDateTime(query.ToString(3))
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

    public static UserToken? GetById(string id)
    {
        var select = GetQueryObj();
        select.WhereEqual("id", id);

        var result = ProcessQuery(select);
        if (result.Count == 0)
            return null;
        return result[0];
    }
    public static UserToken? GetByUser(User user)
    {
        var select = GetQueryObj();
        select.WhereEqual("user_id", user.id);

        var result = ProcessQuery(select);
        if (result.Count == 0)
            return null;
        return result[0];
    }
}

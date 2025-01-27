using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public class SdmGenedGroup : ISdmBaseMethod<GenedGroup>
{
    public static string TableName => "gened_group";

    public static SdmPgsqlQuerySelect GetQueryObj()
    {
        return new SdmPgsqlQuerySelect(TableName);
    }

    public static List<GenedGroup> ProcessQuery(ISdmPgsqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmPgsqlQuery.Execute(queryBuilder);

        var result = new List<GenedGroup>();

        while (query.Next())
        {
            result.Add(new GenedGroup(
                query.ToString(0),
                query.ToString(1)
            ));
            if (!isArray) break;
        }

        query.CleanUp();
        return result;
    }

    public static List<GenedGroup> GetAll()
    {
        var select = GetQueryObj();

        var result = ProcessQuery(select, true);
        return result;
    }

    public static GenedGroup? GetBy(string? id)
    {
        if (id == null)
            return null;

        var select = GetQueryObj();
        select.WhereEqual("id", id);

        var result = ProcessQuery(select);
        if (result.Count == 0)
            return null;
        return result[0];
    }
}

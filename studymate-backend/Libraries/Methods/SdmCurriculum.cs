using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public class SdmCurriculum : ISdmBaseMethod<Curriculum>
{
    public static string TableName => "curriculum";

    public static SdmPgsqlQuerySelect GetQueryObj()
    {
        return new SdmPgsqlQuerySelect(TableName);
    }

    public static List<Curriculum> ProcessQuery(ISdmPgsqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmPgsqlQuery.Execute(queryBuilder);

        var result = new List<Curriculum>();

        while (query.Next())
        {
            result.Add(new Curriculum(
                query.ToInt(0),
                query.ToString(1),
                query.ToString(2),
                query.ToString(3),
                query.ToString(4),
                query.ToString(5),
                query.ToString(6),
                query.ToString(7),
                query.ToString(8)
            ));
            if (!isArray) break;
        }

        query.CleanUp();
        return result;
    }

    public static List<Curriculum> GetAll()
    {
        var select = GetQueryObj();

        var result = ProcessQuery(select, true);
        return result;
    }

    public static Curriculum? GetBy(int id)
    {
        var select = GetQueryObj();
        select.WhereEqual("id", id.ToString());

        var result = ProcessQuery(select);
        if (result.Count == 0)
            return null;
        return result[0];
    }
    public static Curriculum? GetBy(string uniqueId, string year)
    {
        var select = GetQueryObj();
        select.WhereEqual("unique_id", uniqueId);
        select.WhereEqual("year", year);

        var result = ProcessQuery(select);
        if (result.Count == 0)
            return null;
        return result[0];
    }
}

using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public class SdmGenedSubject : ISdmBaseMethod<GenedSubject>
{
    public static string TableName => "gened_subject";

    public static SdmPgsqlQuerySelect GetQueryObj()
    {
        return new SdmPgsqlQuerySelect(TableName);
    }

    public static List<GenedSubject> ProcessQuery(ISdmPgsqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmPgsqlQuery.Execute(queryBuilder);
        
        var result = new List<GenedSubject>();
        
        while (query.Next())
        {
            result.Add(new GenedSubject(
                query.ToString(0),
                query.ToString(1)
            ));
            if (!isArray) break;
        }
        
        query.CleanUp();
        return result;
    }

    public static List<GenedSubject> GetAll()
    {
        var select = GetQueryObj();

        var result = ProcessQuery(select, true);
        return result;
    }
    
}
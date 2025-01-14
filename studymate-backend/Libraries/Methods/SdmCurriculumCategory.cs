using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public abstract class SdmCurriculumCategory : ISdmBaseMethod<CurriculumCategory>
{
    public static string TableName => "cu_curri_category";
    public static SdmPgsqlQuerySelect GetQueryObj()
    {
        return new SdmPgsqlQuerySelect(TableName);
    }
    
    public static List<CurriculumCategory> ProcessQuery(ISdmPgsqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmPgsqlQuery.Execute(queryBuilder);
        var result = new List<CurriculumCategory>();

        while (query.Next())
        {
            result.Add(new CurriculumCategory(
                query.ToInt(0),
                query.ToString(1),
                query.ToString(2),
                query.ToInt(3),
                query.ToInt(4)
            ));
            if (!isArray) break;
        }

        query.CleanUp();
        return result;
    }
    
    public static List<CurriculumCategory> GetAll()
    {
        var select = GetQueryObj();

        var result = ProcessQuery(select, true);
        return result;
    }
    
    public static List<CurriculumCategory> QueryBy(string uniqueId, string year)
    {
        var select = GetQueryObj();
        select.WhereEqual("curri_id", uniqueId);
        select.WhereEqual("year", year);

        var result = ProcessQuery(select, true);
        return result;
    }
}
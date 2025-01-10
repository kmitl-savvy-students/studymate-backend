using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public class SdmCurriculumCategory
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
    
    public static List<CurriculumCategory> GetByCurriAndYear(string curri_id, string year)
    {
        var select = GetQueryObj();
        select.WhereEqual("curri_id", curri_id);
        select.WhereEqual("year", year);

        var result = ProcessQuery(select, true);
        if (result.Count == 0)
            return null;
        return result;
    }
    
    public static List<CurriculumCategory> GetByCurriAndYearAndCat(string curri_id, string year, int cat_id)
    {
        var select = GetQueryObj();
        select.WhereEqual("curri_id", curri_id);
        select.WhereEqual("year", year);
        select.WhereEqual("c_cat_id", cat_id.ToString());

        var result = ProcessQuery(select, true);
        if (result.Count == 0)
            return null;
        return result;
    }

}
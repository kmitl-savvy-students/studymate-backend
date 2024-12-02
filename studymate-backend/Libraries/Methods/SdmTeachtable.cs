using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public class SdmTeachtable : ISdmBaseMethod<Teachtable>
{
    public static string TableName => "teachtable";

    public static SdmPgsqlQuerySelect GetQueryObj()
    {
        return new SdmPgsqlQuerySelect(TableName);
    }

    public static List<Teachtable> ProcessQuery(ISdmPgsqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmPgsqlQuery.Execute(queryBuilder);

        var result = new List<Teachtable>();

        while (query.Next())
        {
            result.Add(new Teachtable(
                query.ToInt(1),
                query.ToInt(2),
                query.ToInt(0)
            ));
            if (!isArray) break;
        }

        query.CleanUp();
        return result;
    }

    public static List<Teachtable> GetAll()
    {
        var select = GetQueryObj();

        var result = ProcessQuery(select, true);
        return result;
    }

    public static Teachtable GetById(int id)
    {
        if (id == null)
            return null;
        
        var select = GetQueryObj();
        select.WhereEqual("id", id.ToString());
        
        var  result = ProcessQuery(select);
        if (result.Count == 0)
            return null;
        return result[0];
    }

    public static void Insert(Teachtable teachtable)
    {
        var insert = new SdmPgsqlQueryInsert(TableName);
        
        insert.Insert("academic_year", teachtable.academic_year.ToString());
        insert.Insert("academic_term", teachtable.academic_term.ToString());
        
        var query = SdmPgsqlQuery.Execute(insert);
        query.CleanUp();
    }
    
}
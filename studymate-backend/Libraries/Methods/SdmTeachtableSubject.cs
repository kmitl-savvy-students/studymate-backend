using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public class SdmTeachtableSubject : ISdmBaseMethod<TeachtableSubject>
{
    public static string TableName => "teachtable_subject";

    public static SdmPgsqlQuerySelect GetQueryObj()
    {
        return new SdmPgsqlQuerySelect(TableName);
    }

    public static List<TeachtableSubject> ProcessQuery(ISdmPgsqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmPgsqlQuery.Execute(queryBuilder);

        var result = new List<TeachtableSubject>();

        while (query.Next())
        {
            result.Add(new TeachtableSubject(
                SdmTeachtable.GetById(query.ToInt(1)),
                query.ToString(2),
                query.ToInt(3),
                query.ToFloat(4),
                query.ToInt(5),
                query.ToInt(0)
                
            ));
            if (!isArray) break;
        }
        query.CleanUp();
        return result;
    }

    public static void Insert(TeachtableSubject teachtableSubject)
    {
        var insert = new SdmPgsqlQueryInsert(TableName);
        
        insert.Insert("teachtable_id", teachtableSubject.teachtable?.id.ToString());
        insert.Insert("subject_id", teachtableSubject.subject_id);
        insert.Insert("interested", teachtableSubject.interested.ToString());
        insert.Insert("rating", teachtableSubject.rating.ToString());
        insert.Insert("count_of_review", teachtableSubject.count_of_review.ToString());

        var query = SdmPgsqlQuery.Execute(insert);
        query.CleanUp();
    }
}
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
                query.ToInt(0), // id
                SdmTeachtable.GetById(query.ToInt(1)), // teachtable
                query.ToString(2), // public_id
                query.ToString(3), // subject_id
                query.ToInt(4), // interested
                query.ToFloat(5) // rating
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
        insert.Insert("public_id", teachtableSubject.public_id);
        insert.Insert("subject_id", teachtableSubject.subject_id);
        insert.Insert("interested", teachtableSubject.interested.ToString());
        insert.Insert("rating", teachtableSubject.rating.ToString());

        var query = SdmPgsqlQuery.Execute(insert);
        query.CleanUp();
    }
}
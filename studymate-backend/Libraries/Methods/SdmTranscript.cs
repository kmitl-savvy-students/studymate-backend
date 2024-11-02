using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Helper;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public class SdmTranscript : ISdmBaseMethod<Transcript>
{
    public static string TableName => "transcript";

    public static SdmPgsqlQuerySelect GetQueryObj()
    {
        return new SdmPgsqlQuerySelect(TableName);
    }

    public static List<Transcript> ProcessQuery(ISdmPgsqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmPgsqlQuery.Execute(queryBuilder);

        var result = new List<Transcript>();

        while (query.Next())
        {
            result.Add(new Transcript(
                query.ToInt(0),
                SdmUser.GetBy(query.ToString(1)),
                SdmCurriculum.GetBy(query.ToInt(2)),
                new SdmDateTime(query.ToString(3))
            ));
            if (!isArray) break;
        }

        query.CleanUp();
        return result;
    }

    public static List<Transcript> GetAll()
    {
        var select = GetQueryObj();

        var result = ProcessQuery(select, true);
        return result;
    }

    public static Transcript? GetBy(int id)
    {
        var select = GetQueryObj();
        select.WhereEqual("id", id.ToString());

        var result = ProcessQuery(select);
        if (result.Count == 0)
            return null;
        return result[0];
    }
    public static Transcript? GetBy(User user)
    {
        var select = GetQueryObj();
        select.WhereEqual("user_id", user.id);

        var result = ProcessQuery(select);
        if (result.Count == 0)
            return null;
        return result[0];
    }

    public static Transcript Insert(Transcript transcript)
    {
        var insert = new SdmPgsqlQueryInsert(TableName);

        insert.Insert("user_id", transcript.user?.id);
        insert.Insert("curriculum_id", transcript.user?.curriculum?.id.ToString());
        insert.Insert("created", transcript.created.ToString());

        var query = SdmPgsqlQuery.Execute(insert);
        transcript.id = query.insertedId;
        query.CleanUp();

        return transcript;
    }
}

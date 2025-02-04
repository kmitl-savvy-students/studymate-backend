using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Helper;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public class SdmTranscript : ISdmBaseMethod<Transcript>
{
    public static string TableName => "transcript";

    public static SdmMysqlQuerySelect GetQueryObj()
    {
        return new SdmMysqlQuerySelect(TableName);
    }

    public static List<Transcript> ProcessQuery(ISdmMysqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmMysqlQuery.Execute(queryBuilder);

        var result = new List<Transcript>();

        while (query.Next())
        {
            result.Add(new Transcript(
                query.ToInt(0),
                SdmUser.GetBy(query.ToString(1)),
                SdmCurriculum.GetBy(query.ToInt(2)),
                new SdmDateTime(query.ToDateTime(3))
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
        select.WhereEqual("user_id", user.Id);

        var result = ProcessQuery(select);
        if (result.Count == 0)
            return null;
        return result[0];
    }
    public static List<Transcript> GetAllBy(User user)
    {
        var select = GetQueryObj();
        select.WhereEqual("user_id", user.Id);

        var result = ProcessQuery(select, true);
        return result;
    }

    public static Transcript Insert(Transcript transcript)
    {
        var insert = new SdmMysqlQueryInsert(TableName);

        insert.Insert("user_id", transcript.user?.Id);
        insert.Insert("curriculum_id", transcript.user?.Curriculum?.id.ToString());
        insert.Insert("created", transcript.created.ToString());

        var query = SdmMysqlQuery.Execute(insert);
        transcript.id = query.InsertedId;
        query.CleanUp();

        return transcript;
    }

    public static void DeleteByUser(User user)
    {
        var delete = new SdmMysqlQueryDelete(TableName);

        delete.WhereEqual("user_id", user.Id);

        var query = SdmMysqlQuery.Execute(delete);
        query.CleanUp();
    }
}

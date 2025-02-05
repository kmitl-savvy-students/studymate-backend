using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Helper;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public abstract class SdmTranscript : ISdmBaseMethod<Transcript>
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
                SdmUser.GetBy(query.ToInt(1)),
                new SdmDateTime(query.ToDateTime(2))
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
    public static List<Transcript> GetAllBy(User user)
    {
        var select = GetQueryObj();
        select.WhereEqual("ts_u_id", user.Id.ToString());

        var result = ProcessQuery(select, true);
        return result;
    }
    public static Transcript? GetBy(int id)
    {
        var select = GetQueryObj();
        select.WhereEqual("ts_id", id.ToString());

        var result = ProcessQuery(select);
        return result.Count == 0 ? null : result[0];
    }
    public static Transcript? GetBy(User user)
    {
        var select = GetQueryObj();
        select.WhereEqual("ts_u_id", user.Id.ToString());

        var result = ProcessQuery(select);
        var transcript = result.Count == 0 ? null : result[0];
        if (transcript == null) return transcript;
        transcript.User = null;
        transcript.Details = SdmTranscriptDetail.GetAllBy(transcript);
        return transcript;
    }

    public static Transcript Insert(Transcript transcript)
    {
        var insert = new SdmMysqlQueryInsert(TableName);

        insert.Insert("ts_u_id", transcript.User?.Id.ToString());
        insert.Insert("ts_date_created", transcript.DateCreated.ToString());

        var query = SdmMysqlQuery.Execute(insert);
        transcript.Id = query.InsertedId;
        query.CleanUp();

        return transcript;
    }
    public static void DeleteBy(User user)
    {
        var delete = new SdmMysqlQueryDelete(TableName);

        delete.WhereEqual("ts_u_id", user.Id.ToString());

        var query = SdmMysqlQuery.Execute(delete);
        query.CleanUp();
    }
    public static void DeleteBy(Transcript transcript)
    {
        var delete = new SdmMysqlQueryDelete(TableName);

        delete.WhereEqual("ts_id", transcript.Id.ToString());

        var query = SdmMysqlQuery.Execute(delete);
        query.CleanUp();
    }
}
using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public class SdmTranscriptData : ISdmBaseMethod<TranscriptData>
{
    public static string TableName => "transcript_data";

    public static SdmPgsqlQuerySelect GetQueryObj()
    {
        return new SdmPgsqlQuerySelect(TableName);
    }

    public static List<TranscriptData> ProcessQuery(ISdmPgsqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmPgsqlQuery.Execute(queryBuilder);

        var result = new List<TranscriptData>();

        while (query.Next())
        {
            result.Add(new TranscriptData(
                query.ToInt(0),
                SdmTranscript.GetBy(query.ToInt(1)),
                SdmSubject.GetBy(query.ToString(2)),
                query.ToInt(3),
                query.ToInt(4),
                query.ToString(5),
                query.ToInt(6)
            ));
            if (!isArray) break;
        }

        query.CleanUp();
        return result;
    }

    public static List<TranscriptData> GetAll()
    {
        var select = GetQueryObj();

        var result = ProcessQuery(select, true);
        return result;
    }

    public static TranscriptData? GetBy(int id)
    {
        var select = GetQueryObj();
        select.WhereEqual("id", id.ToString());

        var result = ProcessQuery(select);
        if (result.Count == 0)
            return null;
        return result[0];
    }
    public static TranscriptData? GetBy(User? user)
    {
        if (user == null)
            return null;

        var select = GetQueryObj();
        select.WhereEqual("user_id", user.id);

        var result = ProcessQuery(select);
        if (result.Count == 0)
            return null;
        return result[0];
    }

    public static void Insert(TranscriptData transcriptData)
    {
        var insert = new SdmPgsqlQueryInsert(TableName);

        insert.Insert("transcript_id", transcriptData.transcript?.id.ToString());
        insert.Insert("subject_id", transcriptData.subject?.id);
        insert.Insert("semester", transcriptData.semester.ToString());
        insert.Insert("year", transcriptData.year.ToString());
        insert.Insert("grade", transcriptData.grade);
        insert.Insert("credit", transcriptData.credit.ToString());

        var query = SdmPgsqlQuery.Execute(insert);
        query.CleanUp();
    }

    public static void DeleteByTranscript(Transcript transcript)
    {
        var delete = new SdmPgsqlQueryDelete(TableName);

        delete.WhereEqual("transcript_id", transcript.id.ToString());

        var query = SdmPgsqlQuery.Execute(delete);
        query.CleanUp();
    }
}

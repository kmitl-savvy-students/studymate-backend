using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public abstract class SdmTranscriptDetail : ISdmBaseMethod<TranscriptDetail>
{
    public static string TableName => "TranscriptDetail";
    public static SdmMysqlQuerySelect GetQueryObj()
    {
        return new SdmMysqlQuerySelect(TableName);
    }
    public static List<TranscriptDetail> ProcessQuery(ISdmMysqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmMysqlQuery.Execute(queryBuilder);

        var result = new List<TranscriptDetail>();
        while (query.Next())
        {
            result.Add(new TranscriptDetail(
                query.ToInt(0),
                SdmTranscript.GetBy(query.ToInt(1)),
                SdmSubject.GetBy(query.ToString(2)),
                SdmTeachtable.GetBy(query.ToInt(3)),
                query.ToString(4),
                query.ToInt(5)
            ));
            if (!isArray) break;
        }

        query.CleanUp();
        return result;
    }

    public static List<TranscriptDetail> GetAll()
    {
        var select = GetQueryObj();

        var result = ProcessQuery(select, true);
        return result;
    }
    public static List<TranscriptDetail> GetAllBy(User? user)
    {
        if (user == null)
            return [];

        var result = new List<TranscriptDetail>();

        var transcripts = SdmTranscript.GetAllBy(user);
        foreach (var transcript in transcripts)
        {
            var select = GetQueryObj();
            select.WhereEqual("TranscriptId", transcript.Id.ToString());

            var transcriptDatas = ProcessQuery(select, true);
            result.AddRange(transcriptDatas);
        }

        return result;
    }
    public static List<TranscriptDetail> GetAllBy(Transcript? transcript)
    {
        if (transcript == null)
            return [];

        var select = GetQueryObj();
        select.WhereEqual("TranscriptId", transcript.Id.ToString());

        var result = ProcessQuery(select, true);
        return result;
    }
    public static TranscriptDetail? GetBy(int id)
    {
        var select = GetQueryObj();
        select.WhereEqual("Id", id.ToString());

        var result = ProcessQuery(select);
        return result.Count == 0 ? null : result[0];
    }

    public static void Insert(TranscriptDetail transcriptDetail)
    {
        var insert = new SdmMysqlQueryInsert(TableName);

        insert.Insert("TranscriptId", transcriptDetail.Transcript?.Id.ToString());
        insert.Insert("SubjectId", transcriptDetail.Subject?.Id.ToString());
        insert.Insert("TeachtableId", transcriptDetail.Teachtable?.Id.ToString());
        insert.Insert("Grade", transcriptDetail.Grade);
        insert.Insert("Credit", transcriptDetail.Credit.ToString());

        var query = SdmMysqlQuery.Execute(insert);
        query.CleanUp();
    }
    public static void DeleteBy(Transcript transcript)
    {
        var delete = new SdmMysqlQueryDelete(TableName);

        delete.WhereEqual("TranscriptId", transcript.Id.ToString());

        var query = SdmMysqlQuery.Execute(delete);
        query.CleanUp();
    }
}
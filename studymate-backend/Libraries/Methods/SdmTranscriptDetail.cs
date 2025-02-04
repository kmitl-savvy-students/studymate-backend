using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public abstract class SdmTranscriptDetail : ISdmBaseMethod<TranscriptDetail>
{
    public static string TableName => "transcript_detail";
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
                query.ToString(4)
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
            select.WhereEqual("tsd_ts_id", transcript.Id.ToString());

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
        select.WhereEqual("tsd_ts_id", transcript.Id.ToString());

        var transcriptDetails = ProcessQuery(select, true);
        foreach (var transcriptDetail in transcriptDetails)
            transcriptDetail.Transcript = null;
        return transcriptDetails;
    }
    public static TranscriptDetail? GetBy(int id)
    {
        var select = GetQueryObj();
        select.WhereEqual("tsd_id", id.ToString());

        var result = ProcessQuery(select);
        return result.Count == 0 ? null : result[0];
    }

    public static void Insert(TranscriptDetail transcriptDetail)
    {
        var insert = new SdmMysqlQueryInsert(TableName);

        insert.Insert("tsd_ts_id", transcriptDetail.Transcript?.Id.ToString());
        insert.Insert("tsd_sbj_id", transcriptDetail.Subject?.Id);
        insert.Insert("tsd_tt_id", transcriptDetail.Teachtable?.Id.ToString());
        insert.Insert("tsd_grade", transcriptDetail.Grade);

        var query = SdmMysqlQuery.Execute(insert);
        query.CleanUp();
    }
    public static void DeleteBy(Transcript transcript)
    {
        var delete = new SdmMysqlQueryDelete(TableName);

        delete.WhereEqual("tsd_ts_id", transcript.Id.ToString());

        var query = SdmMysqlQuery.Execute(delete);
        query.CleanUp();
    }
}
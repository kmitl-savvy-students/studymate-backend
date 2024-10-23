using studymate_backend.Contexts;
using studymate_backend.Models.StudyMate.Object;
using studymate_backend.Models.StudyMate.Raw;

namespace studymate_backend.Services;

public class TranscriptDataService(AppDbContext context)
{
    public TranscriptData? Get(int id)
    {
        var rawTranscriptData = context.TranscriptData.Find(id);
        return rawTranscriptData?.Deserialized();
    }
    public IEnumerable<RawTranscriptData> GetByUser(User user)
    {
        return context.TranscriptData
            .Where(td => td.Transcript != null && td.Transcript.UserId == user.Id)
            .ToList();
    }
    
    public void Add(TranscriptData transcriptData)
    {
        context.TranscriptData.Add(transcriptData.Serialized());
        context.SaveChanges();
    }

    public void Update(TranscriptData transcriptData)
    {
        var updateTranscriptData = context.TranscriptData.Find(transcriptData.Id);

        if (updateTranscriptData == null)
            return;
        
        updateTranscriptData.TranscriptId = transcriptData.TranscriptId;
        updateTranscriptData.SubjectId = transcriptData.SubjectId;
        updateTranscriptData.Semester = transcriptData.Semester;
        updateTranscriptData.Year = transcriptData.Year;
        updateTranscriptData.Grade = transcriptData.Grade;
        updateTranscriptData.Credit = transcriptData.Credit;

        context.SaveChanges();
    }
}
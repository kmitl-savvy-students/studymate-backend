using Newtonsoft.Json;
using studymate_backend.Contexts;
using studymate_backend.Models.StudyMate.Object;

namespace studymate_backend.Services;

public class TranscriptService(AppDbContext context)
{
    public Transcript? Get(int id)
    {
        var rawTranscript = context.Transcript.Find(id);
        return rawTranscript?.Deserialized();
    }

    public int Add(Transcript transcript)
    {
        var serializedTranscript = transcript.Serialized();

        context.Transcript.Add(serializedTranscript);
        context.SaveChanges();

        return serializedTranscript.Id;
    }

    public void Update(Transcript transcript)
    {
        var updateTranscript = context.Transcript.Find(transcript.Id);

        if (updateTranscript == null)
            return;

        updateTranscript.UserId = transcript.UserId;
        updateTranscript.CurriculumId = transcript.CurriculumId;
        updateTranscript.Created = transcript.Created;

        context.SaveChanges();
    }
}
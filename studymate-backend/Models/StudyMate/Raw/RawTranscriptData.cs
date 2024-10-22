using studymate_backend.Models.Core;
using studymate_backend.Models.StudyMate.Object;

namespace studymate_backend.Models.StudyMate.Raw;

public class RawTranscriptData(int id, int transcriptId, string subjectId, string grade) : IBaseModelRaw
{
    public int Id { get; set; } = id;
    public int TranscriptId { get; set; } = transcriptId;
    public string SubjectId { get; set; } = subjectId;
    public string Grade { get; set; } = grade;

    public TranscriptData Deserialized()
    {
        return new TranscriptData(
            Id,
            TranscriptId,
            SubjectId,
            Grade
        );
    }
    
    public RawTranscript? Transcript { get; set; }
}
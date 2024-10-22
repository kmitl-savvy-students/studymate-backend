using studymate_backend.Models.Core;
using studymate_backend.Models.StudyMate.Raw;

namespace studymate_backend.Models.StudyMate.Object;

public class TranscriptData(int id, int transcriptId, string subjectId, string grade) : IBaseModel
{
    public int Id { get; set; } = id;
    public int TranscriptId { get; set; } = transcriptId;
    public string SubjectId { get; set; } = subjectId;
    public string Grade { get; set; } = grade;

    public RawTranscriptData Serialized()
    {
        return new RawTranscriptData(
            Id,
            TranscriptId,
            SubjectId,
            Grade
        );
    }
    
    public Transcript? Transcript { get; set; }
}
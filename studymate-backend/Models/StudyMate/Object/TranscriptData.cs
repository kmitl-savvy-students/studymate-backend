using studymate_backend.Models.Core;
using studymate_backend.Models.StudyMate.Raw;

namespace studymate_backend.Models.StudyMate.Object;

public class TranscriptData(int id, int transcriptId, string subjectId, int semester, int year, string grade, int credit) : IBaseModel
{
    public int Id { get; set; } = id;
    public int TranscriptId { get; set; } = transcriptId;
    public string SubjectId { get; set; } = subjectId;
    public int Semester { get; set; } = semester;
    public int Year { get; set; } = year;
    public string Grade { get; set; } = grade;
    public int Credit { get; set; } = credit;

    public RawTranscriptData Serialized()
    {
        return new RawTranscriptData(
            Id,
            TranscriptId,
            SubjectId,
            Semester,
            Year,
            Grade,
            Credit
        );
    }
    
    public Transcript? Transcript { get; set; }
}
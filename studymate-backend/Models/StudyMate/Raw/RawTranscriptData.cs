using System.Text.Json.Serialization;
using studymate_backend.Models.Core;
using studymate_backend.Models.StudyMate.Object;

namespace studymate_backend.Models.StudyMate.Raw;

public class RawTranscriptData(int id, int transcriptId, string subjectId, int semester, int year, string grade, int credit) : IBaseModelRaw
{
    public int Id { get; set; } = id;
    public int TranscriptId { get; set; } = transcriptId;
    public string SubjectId { get; set; } = subjectId;
    public int Semester { get; set; } = semester;
    public int Year { get; set; } = year;
    public string Grade { get; set; } = grade;
    public int Credit { get; set; } = credit;

    public TranscriptData Deserialized()
    {
        return new TranscriptData(
            Id,
            TranscriptId,
            SubjectId,
            Semester,
            Year,
            Grade,
            Credit
        );
    }
    
    [JsonIgnore]
    public RawTranscript? Transcript { get; set; }
}
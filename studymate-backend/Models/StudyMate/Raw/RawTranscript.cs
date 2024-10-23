using System.Text.Json.Serialization;
using studymate_backend.Models.Core;
using studymate_backend.Models.StudyMate.Object;

namespace studymate_backend.Models.StudyMate.Raw;

public class RawTranscript(int id, string userId, int curriculumId, DateTime created) : IBaseModelRaw
{
    public int Id { get; set; } = id;
    public string UserId { get; set; } = userId;
    public int CurriculumId { get; set; } = curriculumId;
    public DateTime Created { get; set; } = created;

    public Transcript Deserialized()
    {
        return new Transcript(
            Id,
            UserId,
            CurriculumId,
            Created
        );
    }
    
    [JsonIgnore]
    public RawUser? User { get; set; }
    [JsonIgnore]
    public ICollection<RawTranscriptData>? TranscriptData { get; set; }
}
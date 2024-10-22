using studymate_backend.Models.Core;
using studymate_backend.Models.StudyMate.Raw;

namespace studymate_backend.Models.StudyMate.Object;

public class Transcript(int id, string userId, int curriculumId, DateTime created) : IBaseModel
{
    public int Id { get; set; } = id;
    public string UserId { get; set; } = userId;
    public int CurriculumId { get; set; } = curriculumId;
    public DateTime Created { get; set; } = created;

    public RawTranscript Serialized()
    {
        return new RawTranscript(
            Id,
            UserId,
            CurriculumId,
            Created
        );
    }
}
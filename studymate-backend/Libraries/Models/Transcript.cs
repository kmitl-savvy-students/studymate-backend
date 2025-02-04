using studymate_backend.Libraries.Helper;

namespace studymate_backend.Libraries.Models;

public class Transcript(
    int id,
    User? user,
    SdmDateTime dateCreated
) : IBaseModel
{
    public int Id { get; set; } = id;
    public User? User { get; set; } = user;
    public SdmDateTime DateCreated { get; set; } = dateCreated;
    public List<TranscriptDetail> Details { get; set; } = [];
}
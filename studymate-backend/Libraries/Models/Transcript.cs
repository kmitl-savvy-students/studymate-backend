using studymate_backend.Libraries.Helper;

namespace studymate_backend.Libraries.Models;

public class Transcript(
    int id,
    User? user,
    Curriculum? curriculum,
    SdmDateTime created
) : IBaseModel
{
    public int Id { get; set; } = id;
    public User? User { get; set; } = user;
    public Curriculum? Curriculum { get; set; } = curriculum;
    public SdmDateTime Created { get; set; } = created;
}
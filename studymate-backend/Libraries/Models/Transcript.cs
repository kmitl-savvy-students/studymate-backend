using studymate_backend.Libraries.Helper;

namespace studymate_backend.Libraries.Models;

public class Transcript(
    int id,
    User? user,
    Curriculum? curriculum,
    SdmDateTime created
) : IBaseModel
{
    public int id { get; set; } = id;
    public User? user { get; set; } = user;
    public Curriculum? curriculum { get; set; } = curriculum;
    public SdmDateTime created { get; set; } = created;
}
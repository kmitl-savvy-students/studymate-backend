using studymate_backend.Libraries.Helper;

namespace studymate_backend.Libraries.Models;

public class UserToken(
    string id,
    User? user,
    SdmDateTime created,
    SdmDateTime expired
) : IBaseModel
{
    public string id { get; set; } = id;
    public User? user { get; set; } = user;
    public SdmDateTime created { get; set; } = created;
    public SdmDateTime expired { get; set; } = expired;
}

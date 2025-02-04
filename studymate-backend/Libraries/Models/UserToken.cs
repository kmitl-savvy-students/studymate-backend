using studymate_backend.Libraries.Helper;

namespace studymate_backend.Libraries.Models;

public class UserToken(
    string id,
    User? user,
    SdmDateTime dateCreated,
    SdmDateTime dateExpired
) : IBaseModel
{
    public string Id { get; set; } = id;
    public User? User { get; set; } = user;
    public SdmDateTime DateCreated { get; set; } = dateCreated;
    public SdmDateTime DateExpired { get; set; } = dateExpired;
}
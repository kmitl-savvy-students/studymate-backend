using studymate_backend.Helper;
using studymate_backend.Models.StudyMate.Raw;

namespace studymate_backend.Models.StudyMate.Object;

public class UserToken(string id, User user, SdmDateTime created, SdmDateTime expired)
{
    public string Id { get; set; } = id;
    public User User { get; set; } = user;
    public SdmDateTime Created { get; set; } = created;
    public SdmDateTime Expired { get; set; } = expired;

    public RawUserToken Serialized()
    {
        return new RawUserToken(
            Id,
            User.Id,
            Created.ToUTCDateTime(),
            Expired.ToUTCDateTime()
        );
    }
}
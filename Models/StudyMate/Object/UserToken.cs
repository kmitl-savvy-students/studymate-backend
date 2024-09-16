using studymate_backend.Helper;
using studymate_backend.Models.StudyMate.Raw;

namespace studymate_backend.Models.StudyMate.Object;

public class UserToken(string id, User user, SDMDateTime created, SDMDateTime expired)
{
    public string Id { get; set; } = id;
    public User User { get; set; } = user;
    public SDMDateTime Created { get; set; } = created;
    public SDMDateTime Expired { get; set; } = expired;

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
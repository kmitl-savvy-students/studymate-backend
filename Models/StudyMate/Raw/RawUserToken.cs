using studymate_backend.Helper;
using studymate_backend.Models.Core;
using studymate_backend.Models.StudyMate.Object;
using studymate_backend.Services;

namespace studymate_backend.Models.StudyMate.Raw;

public class RawUserToken(string id, string userId, DateTime created, DateTime expired) : BaseModelRaw
{
    public string Id { get; set; } = id;
    public string UserId { get; set; } = userId;
    public DateTime Created { get; set; } = created;
    public DateTime Expired { get; set; } = expired;

    public UserToken? Deserialized(UserService userService)
    {
        var user = userService.Get(UserId);
        if (user == null)
            return null;

        return new UserToken(
            Id,
            user,
            new SDMDateTime(Created),
            new SDMDateTime(Expired)
        );
    }
}
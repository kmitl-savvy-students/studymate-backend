using studymate_backend.Contexts;
using studymate_backend.Enums;
using studymate_backend.Enums.Core;
using studymate_backend.Models.StudyMate.Object;
using studymate_backend.Models.StudyMate.Raw;

namespace studymate_backend.Services;

public class UserService(AppDbContext context)
{
    public IEnumerable<RawUser> GetAll()
    {
        return [.. context.User];
    }

    public User? Get(string id)
    {
        var rawUser = context.User.Find(id);
        if (rawUser == null)
            return null;

        return new User(
            rawUser.Id,
            rawUser.Password,
            BaseEnum.Get<EnumGender>(rawUser.Gender) ?? EnumGender.OTHER,
            rawUser.NameNick,
            rawUser.NameFirst,
            rawUser.NameLast
        );
    }

    public void Add(User user)
    {
        context.User.Add(new RawUser(
            user.Id,
            user.Password,
            user.Gender.GetName(),
            user.NameNick,
            user.NameFirst,
            user.NameLast
        ));
        context.SaveChanges();
    }
}
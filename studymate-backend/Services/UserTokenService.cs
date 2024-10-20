using studymate_backend.Contexts;
using studymate_backend.Models.StudyMate.Object;

namespace studymate_backend.Services;

public class UserTokenService(AppDbContext context, UserService userService)
{
    public UserToken? Get(string id)
    {
        var rawUserToken = context.UserToken.Find(id);
        return rawUserToken?.Deserialized(userService);
    }

    public UserToken? GetByUser(User user)
    {
        var rawUserToken = context.UserToken.FirstOrDefault(userToken => userToken.UserId == user.Id);
        return rawUserToken?.Deserialized(userService);
    }

    public void Remove(UserToken userToken)
    {
        var rawUserToken = context.UserToken.Find(userToken.Id);
        if (rawUserToken == null)
            return;

        context.UserToken.Remove(rawUserToken);
        context.SaveChanges();
    }

    public void Add(UserToken userToken)
    {
        context.UserToken.Add(userToken.Serialized());
        context.SaveChanges();
    }
}
using studymate_backend.Contexts;
using studymate_backend.Models.StudyMate.Object;

namespace studymate_backend.Services;

public class UserService(AppDbContext context)
{
    public User? Get(string id)
    {
        var rawUser = context.User.Find(id);
        return rawUser?.Deserialized();
    }

    public void Add(User user)
    {
        context.User.Add(user.Serialized());
        context.SaveChanges();
    }
}
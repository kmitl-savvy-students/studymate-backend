using Microsoft.EntityFrameworkCore;
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

    public void Update(User user)
    {
        var updateUser = context.User.Find(user.Id);

        if (updateUser == null)
            return;
        
        updateUser.Password = user.Password;
        updateUser.Gender = user.Gender.ToString();
        updateUser.NameNick = user.NameNick;
        updateUser.NameFirst = user.NameFirst;
        updateUser.NameLast = user.NameLast;
        updateUser.Profile = user.Profile;

        context.SaveChanges();
    }
}
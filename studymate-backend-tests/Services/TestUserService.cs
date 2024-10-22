using Microsoft.EntityFrameworkCore;
using studymate_backend.Contexts;
using studymate_backend.Enums;
using studymate_backend.Models.StudyMate.Object;
using studymate_backend.Services;
using Xunit;

namespace Tests.Services;

public class TestUserService
{
    private readonly UserService _userService;

    public TestUserService()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        var context = new AppDbContext(options);

        var user = new User
        (
            "64010009", "no",
            EnumGender.MALE,
            "Korn", "Korn",
            "Rojrattanapanya",
            "no"
        );
        
        context.User.Add(user.Serialized());
        context.SaveChanges();

        _userService = new UserService(context);
    }

    [Fact]
    public void GetUserById_ReturnsUser()
    {
        var user = _userService.Get("64010009");

        Assert.NotNull(user);
        Assert.Equal("Korn", user.NameFirst);
    }
}
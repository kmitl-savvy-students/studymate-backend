using Microsoft.EntityFrameworkCore;
using studymate_backend.Contexts;
using studymate_backend.Controllers;
using studymate_backend.Services;

namespace studymate_backend.Tests;

public static class ControllerFactory
{
    private static AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    public static AuthController GetAuthController()
    {
        var context = GetInMemoryDbContext();
        var userService = new UserService(context);
        var userTokenService = new UserTokenService(context, userService);
        return new AuthController(userService, userTokenService);
    }
}
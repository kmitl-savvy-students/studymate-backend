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

    public static UserController GetUserController()
    {
        var context = GetInMemoryDbContext();
        var userService = new UserService(context);
        return new UserController(context, userService);
    }

    public static AuthController GetAuthController()
    {
        var context = GetInMemoryDbContext();
        var authService = new AuthService(context);
        var userService = new UserService(context);
        return new AuthController(authService, userService);
    }
}
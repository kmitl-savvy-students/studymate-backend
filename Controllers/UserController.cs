using Microsoft.AspNetCore.Mvc;
using studymate_backend.Contexts;
using studymate_backend.Controllers.Core;
using studymate_backend.Enums;
using studymate_backend.Models.Core;
using studymate_backend.Services;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(AppDbContext context, UserService userService) : BaseController
{
    [HttpGet]
    public BaseResponse GetAll()
    {
        return new BaseResponse(EnumResponseCode.OK, userService.GetAll());
    }
}
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Enums;
using studymate_backend.Helper;
using studymate_backend.Models.Core;
using studymate_backend.Services;

namespace studymate_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(UserService userService) : ControllerBase
    {
        [HttpGet]
        public BaseResponse GetAll()
        {
            return new BaseResponse(EnumResponseCode.OK, userService.GetAll());
        }

        [HttpGet("{id}")]
        public BaseResponse Get(string id)
        {
            if (!SDMString.IsValidNumber(id))
                return new BaseResponse(EnumResponseCode.BAD_REQUEST);

            var user = userService.Get(id);
            return user == null ? new BaseResponse(EnumResponseCode.NOT_FOUND) : new BaseResponse(EnumResponseCode.OK, user);
        }
    }
}

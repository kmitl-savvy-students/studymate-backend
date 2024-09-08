using Microsoft.AspNetCore.Mvc;
using studymate_backend.Contexts;
using studymate_backend.Enums;
using studymate_backend.Helper;
using studymate_backend.Models.Core;
using studymate_backend.Services;

namespace studymate_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(AppDbContext context, UserService userService) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly UserService _userService = userService;

		[HttpGet]
		public async Task<IEnumerable<User>> Get()
		{
			return await _userService.GetAllUsersAsync();
		}
	}
        [HttpGet]
        public BaseResponse GetAll()
        {
            return new BaseResponse(EnumResponseCode.OK, _userService.GetAll());
        }

        [HttpGet("{id}")]
        public BaseResponse Get(string id)
        {
            if (!SDMString.IsValidNumber(id))
                return new BaseResponse(EnumResponseCode.BAD_REQUEST);

            var user = _userService.Get(id);
            if (user == null)
            {
                return new BaseResponse(EnumResponseCode.NOT_FOUND);
            }

            return new BaseResponse(EnumResponseCode.OK, user);
        }
    }
}

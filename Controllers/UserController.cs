using Microsoft.AspNetCore.Mvc;
using StudyMate.Models;
using StudyMate.Services;

namespace MyApiProject.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class UserController : ControllerBase
	{
		private readonly UserService _userService;

		public UserController(UserService userService)
		{
			_userService = userService;
		}

		[HttpGet]
		public async Task<IEnumerable<User>> Get()
		{
			return await _userService.GetAllUsersAsync();
		}
	}
}

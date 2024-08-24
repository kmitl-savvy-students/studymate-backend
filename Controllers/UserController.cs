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
			return "Hello, world";
		}

		[HttpPost]
		public async Task<ActionResult<User>> Post(User user)
		{
			var newUser = await _userService.AddUserAsync(user);
			return CreatedAtAction(
				nameof(Get),
				new { id = newUser.Id, username = newUser.Username },
				newUser
			);
		}

		[HttpGet("Home/CustomRoute")]
		public IActionResult HelloWorld()
		{
			return Content("Hello, World!");
		}
	}
}

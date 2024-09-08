using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using StudyMate.Data;
using StudyMate.Models;
using StudyMate.Services;

namespace StudyMate.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class UserController : ControllerBase
	{
		private readonly UserService _userService;
		private readonly UserManagementContext _context;

		public UserController(UserService userService, UserManagementContext context)
		{
			_userService = userService;
			_context = context;
		}

		[HttpGet]
		public async Task<IEnumerable<User>> Get()
		{
			return await _userService.GetAllUsersAsync();
		}
	}
}

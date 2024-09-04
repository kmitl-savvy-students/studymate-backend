using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using StudyMate.Data;
using StudyMate.Models;
using StudyMate.Services;

namespace MyApiProject.Controllers
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

		[HttpPost("signup")]
		public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
		{
			// Hash Password ที่รับมา
			string passwordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password, 13);
			// Check Password กับ ConfirmPassword ว่าตรงกันมั้ย ถ้าไม่ตรงกันจะขึ้น msg = Passwords do not match
			if (!BCrypt.Net.BCrypt.EnhancedVerify(request.PasswordConfirm, passwordHash))
			{
				return BadRequest(new { response_id = 400, message = "Passwords do not match." });
			}

			if (
				string.IsNullOrWhiteSpace(request.Id)
				|| string.IsNullOrWhiteSpace(request.Password)
				|| string.IsNullOrWhiteSpace(request.PasswordConfirm)
				|| string.IsNullOrWhiteSpace(request.Gender)
				|| string.IsNullOrWhiteSpace(request.NameFirst)
				|| string.IsNullOrWhiteSpace(request.NameLast)
				|| string.IsNullOrWhiteSpace(request.NameNick)
			)
			{
				return BadRequest(
					new { response_id = 400, message = "Any Empty Field, Data Too Short" }
				);
			}

			// ตรวจสอบว่าค่า Id ซ้ำกันหรือไม่
			var existingUser = await _context.Users.FindAsync(request.Id);
			if (existingUser != null)
			{
				return BadRequest(new { response_id = 409, message = "ID Duplicattion" });
			}

			// สร้าง User ใหม่
			var newUser = new User
			{
				Id = request.Id,
				Password = passwordHash, // save passwordHash
				Gender = request.Gender,
				NameNick = request.NameNick,
				NameFirst = request.NameFirst,
				NameLast = request.NameLast
			};

			// _context.Users.Add(newUser);
			// await _context.SaveChangesAsync();
			// return Ok(new { response_id = 200, message = "User created successfully." });
			try
			{
				_context.Users.Add(newUser);
				await _context.SaveChangesAsync();
				return Ok(new { response_id = 200, message = "Success" });
			}
			catch (Exception ex)
			{
				// Log the exception
				return StatusCode(
					500,
					new
					{
						response_id = 500,
						message = "Internal Server Error",
						error = ex.Message
					}
				);
			}
		}
	}
}

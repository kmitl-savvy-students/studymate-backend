using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using StudyMate.Data;
using StudyMate.Models;
using StudyMate.Services;

namespace StudyMate.Controllers
{
	[ApiController]
	[Route("user")]
	public class AuthController : ControllerBase
	{
		private readonly AuthService _authService;
		private readonly UserManagementContext _context;

		public AuthController(AuthService authService, UserManagementContext context)
		{
			_authService = authService;
			_context = context;
		}

		[HttpGet("test")]
		public async Task<IEnumerable<SignInToken>> Get()
		{
			return await _authService.GetAllSignInTokensAsync();
		}

		[HttpPost("signup")]
		public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
		{
			if (await _authService.CheckDataIsNull(request) == true)
			{
				return BadRequest(
					new { response_id = 400, message = "Any Empty Field, Data Too Short" }
				);
			}

			if (await _context.Users.FindAsync(request.Id) != null)
			{
				return BadRequest(new { response_id = 409, message = "ID Duplicattion" });
			}

			string hashPassword = await _authService.HashPassword(
				request.Password,
				request.PasswordConfirm
			);

			if (hashPassword == null)
			{
				return BadRequest(
					new { response_id = 400, message = "Password Not match, Too Short, Too Weak" }
				);
			}
			;

			User user = await _authService.createUser(request, hashPassword);

			try
			{
				_context.Users.Add(user);
				await _context.SaveChangesAsync();
				return Ok(new { response_id = 200, message = "Success" });
			}
			catch (Exception ex)
			{
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

		[HttpPost("signin")]
		public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
		{
			User user = await _authService.GetUserByIdAsync(request.Id);
			if (user == null)
			{
				return NotFound(new { response_id = 401, message = "Invalid id or password" });
			}

			if (!BCrypt.Net.BCrypt.EnhancedVerify(request.Password, user.Password))
			{
				return NotFound(new { response_id = 401, message = "Invalid id or password" });
			}

			string token = await _authService.createToken();
			SignInToken signInToken = await _authService.createSignInToken(token, user);
			try
			{
				_context.SignInTokens.Add(signInToken);
				await _context.SaveChangesAsync();
				return Ok(new { response_id = 200, message = token });
			}
			catch (Exception ex)
			{
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

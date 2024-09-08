using Microsoft.AspNetCore.Mvc;
using studymate_backend.Models;
using studymate_backend.Services;

namespace studymate_backend.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController(AuthService authService) : ControllerBase
	{
		[HttpPost("signup")]
		public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
		{
			if (authService.CheckDataIsNull(request))
			{
				
				return BadRequest(
					new { response_id = 400, message = "Any Empty Field, Data Too Short" }
				);
			}
		
			if (await authService.GetRawUserByIdAsync(request.Id) != null)
			{
				return BadRequest(new { response_id = 409, message = "ID Duplication" });
			}
		
			if (request.Password != request.PasswordConfirm)
			{
				return BadRequest(
					new { response_id = 400, message = "Password Not match, Too Short, Too Weak" }
				);
			}
		
			var hashPassword = authService.HashPassword(request.Password);
		
			try
			{
				authService.CreateRawUser(request, hashPassword);
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
			var user = await authService.GetRawUserByIdAsync(request.Id);
			if (user == null)
			{
				return NotFound(new { response_id = 401, message = "Invalid id or password" });
			}
		
			if (!authService.CompareHashPassword(request.Password, user.Password))
			{
				return NotFound(new { response_id = 401, message = "Invalid id or password" });
			}
		
			var token = authService.CreateToken();
			try
			{
				var signInToken = authService.CreateSignInToken(token, user);
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

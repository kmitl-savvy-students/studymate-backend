using Microsoft.EntityFrameworkCore;
using StudyMate.Data;
using StudyMate.Models;

namespace StudyMate.Services
{
	public class AuthService
	{
		private readonly UserManagementContext _context;

		public AuthService(UserManagementContext context)
		{
			_context = context;
		}

		public async Task<IEnumerable<SignInToken>> GetAllSignInTokensAsync()
		{
			return await _context.SignInTokens.ToListAsync();
		}

		public async Task<bool> CheckDataIsNull(SignUpRequest signUpRequest)
		{
			if (
				string.IsNullOrWhiteSpace(signUpRequest.Id)
				|| string.IsNullOrWhiteSpace(signUpRequest.Password)
				|| string.IsNullOrWhiteSpace(signUpRequest.PasswordConfirm)
				|| string.IsNullOrWhiteSpace(signUpRequest.Gender)
				|| string.IsNullOrWhiteSpace(signUpRequest.NameFirst)
				|| string.IsNullOrWhiteSpace(signUpRequest.NameLast)
				|| string.IsNullOrWhiteSpace(signUpRequest.NameNick)
			)
			{
				return true;
			}
			return false;
		}

		public async Task<string> HashPassword(string password, string passwordConfirm)
		{
			if (password != passwordConfirm)
			{
				return null;
			}
			string passwordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(password, 13);
			return passwordHash;
		}

		public async Task<User> createUser(SignUpRequest signUpRequest, string passwordHash)
		{
			var user = new User
			{
				Id = signUpRequest.Id,
				Password = passwordHash, // save passwordHash
				Gender = signUpRequest.Gender,
				NameNick = signUpRequest.NameNick,
				NameFirst = signUpRequest.NameFirst,
				NameLast = signUpRequest.NameLast
			};
			return user;
		}

		public async Task<string> createToken()
		{
			var token = Guid.NewGuid().ToString() + Guid.NewGuid().ToString();
			token = token.Replace("-", "");
			token = token.Substring(0, 64);
			return token;
		}

		public async Task<SignInToken> createSignInToken(string token, User user)
		{
			var signInToken = new SignInToken
			{
				Id = token,
				UserId = user.Id,
				TimeCreated = DateTime.UtcNow,
				TimeExpired = DateTime.UtcNow.AddHours(1)
			};
			return signInToken;
		}

		public async Task<User?> GetUserByIdAsync(string id)
		{
			return await _context.Users.FindAsync(id);
		}
	}
}

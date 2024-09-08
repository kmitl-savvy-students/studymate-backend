using Microsoft.EntityFrameworkCore;
using studymate_backend.Contexts;
using studymate_backend.Models.StudyMate.Raw;
using studymate_backend.Models;

namespace studymate_backend.Services
{
	public class AuthService(AppDbContext context)
	{

		public bool CheckDataIsNull(SignUpRequest signUpRequest)
		{
			return string.IsNullOrWhiteSpace(signUpRequest.Id)
			       || string.IsNullOrWhiteSpace(signUpRequest.Password)
			       || string.IsNullOrWhiteSpace(signUpRequest.PasswordConfirm)
			       || string.IsNullOrWhiteSpace(signUpRequest.Gender)
			       || string.IsNullOrWhiteSpace(signUpRequest.NameFirst)
			       || string.IsNullOrWhiteSpace(signUpRequest.NameLast)
			       || string.IsNullOrWhiteSpace(signUpRequest.NameNick);
		}

		public string HashPassword(string password)
		{
			return BCrypt.Net.BCrypt.EnhancedHashPassword(password, 13);
		}

		public bool CompareHashPassword(string password, string hash)
		{
			return BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
		}

		public RawUser CreateRawUser(SignUpRequest signUpRequest, string passwordHash)
		{
			var user = new RawUser
			{
				Id = signUpRequest.Id,
				Password = passwordHash, // save passwordHash
				Gender = signUpRequest.Gender,
				NameNick = signUpRequest.NameNick,
				NameFirst = signUpRequest.NameFirst,
				NameLast = signUpRequest.NameLast
			};
			
			context.User.Add(user);
			context.SaveChanges();
			
			return user;
		}

		public string CreateToken()
		{
			var token = Guid.NewGuid().ToString() + Guid.NewGuid().ToString();
			token = token.Replace("-", "");
			token = token.Substring(0, 64);
			
			return token;
		}

		public SignInToken CreateSignInToken(string token, RawUser user)
		{
			var signInToken = new SignInToken
			{
				Id = token,
				UserId = user.Id,
				TimeCreated = DateTime.UtcNow,
				TimeExpired = DateTime.UtcNow.AddHours(1)
			};
			
			context.SignInToken.Add(signInToken);
			context.SaveChanges();
			
			return signInToken;
		}

		public async Task<RawUser?> GetRawUserByIdAsync(string id)
		{
			return await context.User.FindAsync(id);
		}
	}
}

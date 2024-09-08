using Microsoft.EntityFrameworkCore;
using StudyMate.Data;
using StudyMate.Models;

namespace StudyMate.Services
{
	public class UserService
	{
		private readonly UserManagementContext _context;

		public UserService(UserManagementContext context)
		{
			_context = context;
		}

		public async Task<IEnumerable<User>> GetAllUsersAsync()
		{
			return await _context.Users.ToListAsync();
		}

		public async Task<User?> GetUserByIdAsync(string id)
		{
			return await _context.Users.FindAsync(id);
		}
	}
}

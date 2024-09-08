using Microsoft.EntityFrameworkCore;
using StudyMate.Models;

namespace StudyMate.Data
{
	public class UserManagementContext : DbContext
	{
		public UserManagementContext(DbContextOptions<UserManagementContext> options)
			: base(options) { }

		public DbSet<User> Users { get; set; }

		public DbSet<SignInToken> SignInTokens { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<User>().ToTable("user", "user_management");
			modelBuilder.Entity<SignInToken>().ToTable("signin_token", "user_management");
		}
	}
}

using Microsoft.EntityFrameworkCore;
using studymate_backend.Models;
using studymate_backend.Models.StudyMate.Raw;

namespace studymate_backend.Contexts
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<RawUser> User { get; set; }
        public DbSet<SignInToken> SignInToken { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RawUser>().ToTable("user", "user_management");
            modelBuilder.Entity<SignInToken>().ToTable("signin_token", "user_management");
        }
    }
    

}
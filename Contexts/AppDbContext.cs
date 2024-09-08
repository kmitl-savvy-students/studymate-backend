using Microsoft.EntityFrameworkCore;
using studymate_backend.Models.StudyMate.Raw;

namespace studymate_backend.Contexts
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<RawUser> User { get; set; }
    }
}

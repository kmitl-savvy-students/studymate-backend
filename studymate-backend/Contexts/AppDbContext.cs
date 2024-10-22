using Microsoft.EntityFrameworkCore;
using studymate_backend.Models.StudyMate.Raw;

namespace studymate_backend.Contexts;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<RawUser> User { get; init; }
    public DbSet<RawUserToken> UserToken { get; init; }
}
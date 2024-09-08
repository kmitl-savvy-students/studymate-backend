using studymate_backend.Contexts;
using studymate_backend.Models.StudyMate.Raw;

namespace studymate_backend.Services
{
    public class UserService(AppDbContext context)
    {
        private readonly AppDbContext _context = context;

        public IEnumerable<RawUser> GetAll()
        {
            return [.. _context.User];
        }
        public RawUser? Get(string id)
        {
            return _context.User.Find(id);
        }
    }
}

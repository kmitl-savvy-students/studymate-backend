using studymate_backend.Models.Core;

namespace studymate_backend.Models.StudyMate.Raw
{
    public class RawUser : BaseModelRaw
    {
        public required string Id { get; set; }
        public required string Password { get; set; }
        public string? Gender { get; set; }
        public string? NameNick { get; set; }
        public string? NameFirst { get; set; }
        public string? NameLast { get; set; }
    }
}

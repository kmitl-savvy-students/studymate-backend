using studymate_backend.Models.Core;

// ReSharper disable All

namespace studymate_backend.Models.StudyMate.Raw;

public class RawUser(string id, string password, string gender, string nameNick, string nameFirst, string nameLast) : BaseModelRaw
{
    public string Id { get; init; } = id;
    public string Password { get; init; } = password;
    public string Gender { get; init; } = gender;
    public string NameNick { get; init; } = nameNick;
    public string NameFirst { get; init; } = nameFirst;
    public string NameLast { get; init; } = nameLast;
}
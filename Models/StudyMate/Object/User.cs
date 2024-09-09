using studymate_backend.Enums;
using studymate_backend.Models.Core;

namespace studymate_backend.Models.StudyMate.Object;

public class User(string id, string password, EnumGender gender, string nameNick, string nameFirst, string nameLast) : BaseModel
{
    public string Id { get; } = id;
    public string Password { get; } = password;
    public EnumGender Gender { get; } = gender;
    public string NameNick { get; } = nameNick;
    public string NameFirst { get; } = nameFirst;
    public string NameLast { get; } = nameLast;
}
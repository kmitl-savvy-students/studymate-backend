using studymate_backend.Enums;
using studymate_backend.Enums.Core;
using studymate_backend.Models.Core;
using studymate_backend.Models.StudyMate.Object;

namespace studymate_backend.Models.StudyMate.Raw;

public class RawUser(string id, string password, string gender, string nameNick, string nameFirst, string nameLast) : BaseModelRaw
{
    public string Id { get; set; } = id;
    public string Password { get; set; } = password;
    public string Gender { get; set; } = gender;
    public string NameNick { get; set; } = nameNick;
    public string NameFirst { get; set; } = nameFirst;
    public string NameLast { get; set; } = nameLast;

    public User Deserialized()
    {
        return new User(
            Id,
            Password,
            BaseEnum.Get<EnumGender>(Gender) ?? EnumGender.OTHER,
            NameNick,
            NameFirst,
            NameLast
        );
    }
}
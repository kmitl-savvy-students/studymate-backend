using studymate_backend.Enums;
using studymate_backend.Models.Core;
using studymate_backend.Models.StudyMate.Raw;

namespace studymate_backend.Models.StudyMate.Object;

public class User(string id, string password, EnumGender gender, string nameNick, string nameFirst, string nameLast, string profile) : IBaseModel
{
    public string Id { get; set; } = id;
    public string Password { get; set; } = password;
    public EnumGender Gender { get; set; } = gender;
    public string NameNick { get; set; } = nameNick;
    public string NameFirst { get; set; } = nameFirst;
    public string NameLast { get; set; } = nameLast;
    public string Profile { get; set; } = profile;

    public RawUser Serialized()
    {
        return new RawUser(
            Id,
            Password,
            Gender.GetName(),
            NameNick,
            NameFirst,
            NameLast,
            Profile
        );
    }
}
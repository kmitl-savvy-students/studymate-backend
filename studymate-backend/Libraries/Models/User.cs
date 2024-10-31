using studymate_backend.Libraries.Enums;

namespace studymate_backend.Libraries.Models;

public class User(
    string id,
    string password,
    EnumGender gender,
    string nameNick,
    string nameFirst,
    string nameLast,
    string profile,
    Curriculum? curriculum
) : IBaseModel
{
    public string id { get; set; } = id;
    public string password { get; set; } = password;
    public EnumGender gender { get; set; } = gender;
    public string nameNick { get; set; } = nameNick;
    public string nameFirst { get; set; } = nameFirst;
    public string nameLast { get; set; } = nameLast;
    public string profile { get; set; } = profile;
    public Curriculum? curriculum { get; set; } = curriculum;
}
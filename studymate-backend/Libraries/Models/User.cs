namespace studymate_backend.Libraries.Models;

public class User(
    string id,
    string password,
    string nameNick,
    string nameFirst,
    string nameLast,
    string profile,
    Curriculum? curriculum
) : IBaseModel
{
    public string id { get; set; } = id;
    public string password { get; set; } = password;
    public string nameNick { get; set; } = nameNick;
    public string nameFirst { get; set; } = nameFirst;
    public string nameLast { get; set; } = nameLast;
    public string profile { get; set; } = profile;
    public Curriculum? curriculum { get; set; } = curriculum;

    public string GetFullName()
    {
        return nameFirst + " " + nameLast;
    }
}

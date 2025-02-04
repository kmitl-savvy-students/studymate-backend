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
    public string Id { get; } = id;
    public string Password { get; } = password;
    public string NameNick { get; set; } = nameNick;
    public string NameFirst { get; set; } = nameFirst;
    public string NameLast { get; set; } = nameLast;
    public string Profile { get; set; } = profile;
    public Curriculum? Curriculum { get; set; } = curriculum;

    public string GetFullName()
    {
        return NameFirst + " " + NameLast;
    }
}

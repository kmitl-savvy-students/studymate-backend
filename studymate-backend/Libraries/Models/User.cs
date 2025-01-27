namespace studymate_backend.Libraries.Models;

public class User(
    int id,
    string password,
    string nameNick,
    string nameFirst,
    string nameLast,
    string profile,
    bool isAdmin,
    Curriculum? curriculum
) : IBaseModel
{
    public int Id { get; } = id;
    public string Password { get; } = password;
    public string NameNick { get; set; } = nameNick;
    public string NameFirst { get; set; } = nameFirst;
    public string NameLast { get; set; } = nameLast;
    public string Profile { get; set; } = profile;
    public bool IsAdmin { get; set; } = isAdmin;
    public Curriculum? Curriculum { get; set; } = curriculum;

    public string GetFullName()
    {
        return NameFirst + " " + NameLast;
    }
}
namespace studymate_backend.Libraries.Models;

public class User(
    int id,
    string password,
    string nickname,
    string firstname,
    string lastname,
    string profilePicture,
    bool isAdmin,
    Curriculum? curriculum,
    int viewPolicy
) : IBaseModel
{
    public int Id { get; } = id;
    public string Password { get; } = password;
    public string Nickname { get; set; } = nickname;
    public string Firstname { get; set; } = firstname;
    public string Lastname { get; set; } = lastname;
    public string ProfilePicture { get; set; } = profilePicture;
    public bool IsAdmin { get; set; } = isAdmin;
    public Curriculum? Curriculum { get; set; } = curriculum;
    public int ViewPolicy { get; set; } = viewPolicy;

    public string GetFullName()
    {
        return Firstname + " " + Lastname;
    }
}
namespace studymate_backend.Models.StudyMate.Raw.Request.Auth;

public class RequestSignUp
{
    public string? Id { get; set; }
    public string? Password { get; set; }
    public string? PasswordConfirm { get; set; }
    public string? NameNick { get; set; }
    public string? NameFirst { get; set; }
    public string? NameLast { get; set; }
    public string? Gender { get; set; }
}
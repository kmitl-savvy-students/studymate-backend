namespace studymate_backend.Libraries.Models;

public class TranscriptDetail(
    int id,
    Transcript? transcript,
    Subject? subject,
    Teachtable? teachtable,
    string grade
) : IBaseModel
{
    public int Id { get; set; } = id;
    public Transcript? Transcript { get; set; } = transcript;
    public Subject? Subject { get; set; } = subject;
    public Teachtable? Teachtable { get; set; } = teachtable;
    public string Grade { get; set; } = grade;
}
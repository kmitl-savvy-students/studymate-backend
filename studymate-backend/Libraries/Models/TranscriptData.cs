namespace studymate_backend.Libraries.Models;

public class TranscriptData(
    int id,
    Transcript? transcript,
    Subject? subject,
    int semester,
    int year,
    string grade,
    int credit
) : IBaseModel
{
    public int id { get; set; } = id;
    public Transcript? transcript { get; set; } = transcript;
    public Subject? subject { get; set; } = subject;
    public int semester { get; set; } = semester;
    public int year { get; set; } = year;
    public string grade { get; set; } = grade;
    public int credit { get; set; } = credit;
}

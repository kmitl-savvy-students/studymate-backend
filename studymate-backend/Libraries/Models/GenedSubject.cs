namespace studymate_backend.Libraries.Models;

public class GenedSubject(
    string subjectId,
    GenedGroup? group
) : IBaseModel
{
    public string subjectId { get; set; } = subjectId;
    public GenedGroup? group { get; set; } = group;
}

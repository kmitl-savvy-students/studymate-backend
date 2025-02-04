namespace studymate_backend.Libraries.Models;

public class CurriculumGroupSubject(
    int id,
    CurriculumGroup? group,
    Subject? subject
) : IBaseModel
{
    public int Id { get; set; } = id;
    public CurriculumGroup? Group { get; set; } = group;
    public Subject? Subject { get; set; } = subject;
}
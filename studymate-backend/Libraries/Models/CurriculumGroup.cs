namespace studymate_backend.Libraries.Models;

public class CurriculumGroup(
    int id,
    int? parentId,
    string type,
    string name,
    int credit,
    List<CurriculumGroup> children,
    List<CurriculumGroupSubject> subjects) : IBaseModel
{
    public int Id { get; set; } = id;
    public int? ParentId { get; set; } = parentId;
    public string Type { get; set; } = type;
    public string Name { get; set; } = name;
    public int Credit { get; set; } = credit;
    public List<CurriculumGroup> Children { get; set; } = children;
    public List<CurriculumGroupSubject> Subjects { get; set; } = subjects;
}
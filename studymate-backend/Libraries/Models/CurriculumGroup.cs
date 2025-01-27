using studymate_backend.Libraries.Enums;

namespace studymate_backend.Libraries.Models;

public class CurriculumGroup(
    int id,
    CurriculumGroup? parent,
    EnumGroupType type,
    string name
) : IBaseModel
{
    public int Id { get; set; } = id;
    public CurriculumGroup? Parent { get; set; } = parent;
    public EnumGroupType Type { get; set; } = type;
    public string Name { get; set; } = name;
}
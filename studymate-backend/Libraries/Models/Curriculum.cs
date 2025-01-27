namespace studymate_backend.Libraries.Models;

public class Curriculum(
    int id,
    CurriculumType? type,
    int year,
    string nameTh,
    string nameEn,
    CurriculumGroup? group
) : IBaseModel
{
    public int Id { get; } = id;
    public CurriculumType? Type { get; set; } = type;
    public int Year { get; set; } = year;
    public string NameTh { get; set; } = nameTh;
    public string NameEn { get; set; } = nameEn;
    public CurriculumGroup? Group { get; set; } = group;
}
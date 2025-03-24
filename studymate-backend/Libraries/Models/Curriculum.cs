namespace studymate_backend.Libraries.Models;

public class Curriculum(
    int id,
    bool isVisible,
    Program? program,
    int year,
    string nameTh,
    string nameEn,
    CurriculumGroup? curriculumGroup
) : IBaseModel
{
    public int Id { get; } = id;
    public bool IsVisible { get; set; } = isVisible;
    public Program? Program { get; set; } = program;
    public int Year { get; set; } = year;
    public string NameTh { get; set; } = nameTh;
    public string NameEn { get; set; } = nameEn;
    public CurriculumGroup? CurriculumGroup { get; set; } = curriculumGroup;
}
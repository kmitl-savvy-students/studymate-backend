namespace studymate_backend.Libraries.Models;

public class Curriculum(
    int id,
    string kmitlId,
    Program? program,
    int year,
    string nameTh,
    string nameEn,
    CurriculumGroup? curriculumGroup
) : IBaseModel
{
    public int Id { get; } = id;
    public string KmitlId { get; set; } = kmitlId;
    public Program? Program { get; set; } = program;
    public int Year { get; set; } = year;
    public string NameTh { get; set; } = nameTh;
    public string NameEn { get; set; } = nameEn;
    public CurriculumGroup? CurriculumGroup { get; set; } = curriculumGroup;
}
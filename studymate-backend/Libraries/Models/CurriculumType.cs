namespace studymate_backend.Libraries.Models;

public class CurriculumType(
    int id,
    Department? department,
    string nameTh,
    string nameEn
) : IBaseModel
{
    public int Id { get; set; } = id;
    public Department? Department { get; set; } = department;
    public string NameTh { get; set; } = nameTh;
    public string NameEn { get; set; } = nameEn;
}
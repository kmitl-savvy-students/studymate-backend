namespace studymate_backend.Libraries.Models;

public class Subject(
    int id,
    string nameTh,
    string nameEn,
    int credit,
    string description
) : IBaseModel
{
    public int Id { get; } = id;
    public string NameTh { get; set; } = nameTh;
    public string NameEn { get; set; } = nameEn;
    public int Credit { get; set; } = credit;
    public string Description { get; set; } = description;
}
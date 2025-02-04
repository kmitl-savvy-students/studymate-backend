namespace studymate_backend.Libraries.Models;

public class Subject(
    string id,
    string nameTh,
    string nameEn,
    int credit,
    string detail
) : IBaseModel
{
    public string Id { get; } = id;
    public string NameTh { get; set; } = nameTh;
    public string NameEn { get; set; } = nameEn;
    public int Credit { get; set; } = credit;
    public string Detail { get; set; } = detail;
}
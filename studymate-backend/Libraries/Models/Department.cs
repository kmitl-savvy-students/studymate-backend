namespace studymate_backend.Libraries.Models;

public class Department(
    int id,
    string kmitlId,
    Faculty? faculty,
    string nameTh,
    string nameEn
) : IBaseModel
{
    public int Id { get; } = id;
    public string KmitlId { get; set; } = kmitlId;
    public Faculty? Faculty { get; set; } = faculty;
    public string NameTh { get; set; } = nameTh;
    public string NameEn { get; set; } = nameEn;
}
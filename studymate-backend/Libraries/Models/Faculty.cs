namespace studymate_backend.Libraries.Models;

public class Faculty(
    int id,
    string nameTh,
    string nameEn
) : IBaseModel
{
    public int Id { get; } = id;
    public string NameTh { get; set; } = nameTh;
    public string NameEn { get; set; } = nameEn;
}
namespace studymate_backend.Libraries.Models;

public class Program(
    int id,
    bool isVisible,
    string kmitlId,
    Department? department,
    string nameTh,
    string nameEn
) : IBaseModel
{
    public int Id { get; set; } = id;
    public bool IsVisible { get; set; } = isVisible;
    public string KmitlId { get; set; } = kmitlId;
    public Department? Department { get; set; } = department;
    public string NameTh { get; set; } = nameTh;
    public string NameEn { get; set; } = nameEn;
}
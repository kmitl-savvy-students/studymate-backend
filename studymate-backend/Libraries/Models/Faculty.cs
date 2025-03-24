namespace studymate_backend.Libraries.Models;

public class Faculty(
    int id,
    int isVisible,
    string kmitlId,
    string nameTh,
    string nameEn
) : IBaseModel
{
    public int Id { get; } = id;
    public int IsVisible { get; set; } = 0;
    public string KmitlId { get; set; } = kmitlId;
    public string NameTh { get; set; } = nameTh;
    public string NameEn { get; set; } = nameEn;
}
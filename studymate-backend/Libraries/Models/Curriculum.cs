namespace studymate_backend.Libraries.Models;

public class Curriculum(
    int id,
    string uniqueId,
    string year,
    string nameTh,
    string nameEn,
    string degreeNameTh,
    string degreeNameThShort,
    string degreeNameEn,
    string degreeNameEnShort,
    string pid
) : IBaseModel
{
    public int id { get; set; } = id;
    public string uniqueId { get; set; } = uniqueId;
    public string year { get; set; } = year;
    public string nameTh { get; set; } = nameTh;
    public string nameEn { get; set; } = nameEn;
    public string degreeNameTh { get; set; } = degreeNameTh;
    public string degreeNameThShort { get; set; } = degreeNameThShort;
    public string degreeNameEn { get; set; } = degreeNameEn;
    public string degreeNameEnShort { get; set; } = degreeNameEnShort;
    public string pid { get; set; } = pid;
}
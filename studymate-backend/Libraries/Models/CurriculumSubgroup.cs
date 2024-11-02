namespace studymate_backend.Libraries.Models;

public class CurriculumSubgroup(
    int categoryId,
    int groupId,
    int subgroupId,
    string uniqueId,
    string year,
    string subgroupName,
    int credit1,
    int credit2,
    string condition,
    string link
) : IBaseModel
{
    public int categoryId { get; set; } = categoryId;
    public int groupId { get; set; } = groupId;
    public int subgroupId { get; set; } = subgroupId;
    public string uniqueId { get; set; } = uniqueId;
    public string year { get; set; } = year;
    public string subgroupName { get; set; } = subgroupName;
    public int credit1 { get; set; } = credit1;
    public int credit2 { get; set; } = credit2;
    public string condition { get; set; } = condition;
    public string link { get; set; } = link;
}

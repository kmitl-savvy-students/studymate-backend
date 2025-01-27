namespace studymate_backend.Libraries.Models;

public class CurriculumGroup(
    int categoryId,
    int groupId,
    string curriculumId,
    string year,
    string groupName,
    int credit1,
    int credit2,
    string subgroupFlag,
    string condition,
    string link
) : IBaseModel
{
    public int categoryId { get; set; } = categoryId;
    public int groupId { get; set; } = groupId;
    public string curriculumId { get; set; } = curriculumId;
    public string year { get; set; } = year;
    public string groupName { get; set; } = groupName;
    public int credit1 { get; set; } = credit1;
    public int credit2 { get; set; } = credit2;
    public string subgroupFlag { get; set; } = subgroupFlag;
    public string condition { get; set; } = condition;
    public string link { get; set; } = link;
}

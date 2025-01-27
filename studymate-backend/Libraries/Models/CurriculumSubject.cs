namespace studymate_backend.Libraries.Models;

public class CurriculumSubject(
    Subject? subject,
    int categoryId,
    int groupId,
    int subgroupId,
    string uniqueId,
    string year,
    string normalFlag,
    string coopFlag
) : IBaseModel
{
    public Subject? subject { get; set; } = subject;
    public int categoryId { get; set; } = categoryId;
    public int groupId { get; set; } = groupId;
    public int subgroupId { get; set; } = subgroupId;
    public string uniqueId { get; set; } = uniqueId;
    public string year { get; set; } = year;
    public string normalFlag { get; set; } = normalFlag;
    public string coopFlag { get; set; } = coopFlag;
}

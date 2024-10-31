namespace studymate_backend.Libraries.Models;

public class CurriculumGroup(
    int c_cat_id,
    int c_group_id,
    string curri_id,
    string year,
    string c_group_name,
    int credit1,
    int credit2,
    string subgroup_flag,
    string condition,
    string link
) : IBaseModel
{
    public int c_cat_id { get; set; } = c_cat_id;
    public int c_group_id { get; set; } = c_group_id;
    public string curri_id { get; set; } = curri_id;
    public string year { get; set; } = year;
    public string c_group_name { get; set; } = c_group_name;
    public int credit1 { get; set; } = credit1;
    public int credit2 { get; set; } = credit2;
    public string subgroup_flag { get; set; } = subgroup_flag;
    public string condition { get; set; } = condition;
    public string link { get; set; } = link;
}
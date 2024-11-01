namespace studymate_backend.Libraries.Models;

public class CurriculumSubject(
    string subject_id,
    int c_cat_id,
    int c_group_id,
    int c_subgroup_id,
    string curri_id,
    string year,
    string normal_flag,
    string coop_flag
) : IBaseModel
{
    public string subject_id { get; set; } = subject_id;
    public int c_cat_id { get; set; } = c_cat_id;
    public int c_group_id { get; set; } = c_group_id;
    public int c_subgroup_id { get; set; } = c_subgroup_id;
    public string curri_id { get; set; } = curri_id;
    public string year { get; set; } = year;
    public string normal_flag { get; set; } = normal_flag;
    public string coop_flag { get; set; } = coop_flag;
}
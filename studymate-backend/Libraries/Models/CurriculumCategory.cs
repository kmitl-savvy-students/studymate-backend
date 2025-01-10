namespace studymate_backend.Libraries.Models;

public class CurriculumCategory(
    int c_cat_id,
    string curri_id,
    string year,
    int credit1,
    int credit2
) : IBaseModel
{
    public int c_cat_id { get; set; } = c_cat_id;
    public string curri_id { get; set; } = curri_id;
    public string year { get; set; } = year;
    public int credit1 { get; set; } = credit1;
    public int credit2 { get; set; } = credit2;
}
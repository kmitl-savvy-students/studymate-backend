namespace studymate_backend.Libraries.Models;

public class CurriculumIndex(
    string curri_id,
    string curri_name,
    string curri_ename,
    string faculty_id,
    string dept_id,
    string level,
    string status,
    int curr_type
) : IBaseModel
{
    public string curri_id { get; set; } = curri_id;
    public string curri_name { get; set; } = curri_name;
    public string curri_ename { get; set; } = curri_ename;
    public string faculty_id { get; set; } = faculty_id;
    public string dept_id { get; set; } = dept_id;
    public string level { get; set; } = level;
    public string status { get; set; } = status;
    public int curr_type { get; set; } = curr_type;
}
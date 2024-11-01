namespace studymate_backend.Libraries.Models;

public class Subject(
    string subject_id,
    string subject_tname,
    string subject_ename,
    int credit,
    int lect_hr,
    int prac_hr,
    string prerequisite,
    string detail,
    int self_hr,
    string prerequisite2,
    int lock_ed,
    int precondition,
    string status,
    string subject_type,
    string prerequisite3,
    string prerequisite4,
    string prerequisite5,
    DateTime last_modified
) : IBaseModel
{
    public string subject_id { get; set; } = subject_id;
    public string subject_tname { get; set; } = subject_tname;
    public string subject_ename { get; set; } = subject_ename;
    public int credit { get; set; } = credit;
    public int lect_hr { get; set; } = lect_hr;
    public int prac_hr { get; set; } = prac_hr;
    public string prerequisite { get; set; } = prerequisite;
    public string detail { get; set; } = detail;
    public int self_hr { get; set; } = self_hr;
    public string prerequisite2 { get; set; } = prerequisite2;
    public int lock_ed { get; set; } = lock_ed;
    public int precondition { get; set; } = precondition;
    public string status { get; set; } = status;
    public string subject_type { get; set; } = subject_type;
    public string prerequisite3 { get; set; } = prerequisite3;
    public string prerequisite4 { get; set; } = prerequisite4;
    public string prerequisite5 { get; set; } = prerequisite5;
    public DateTime last_modified { get; set; } = last_modified;
}
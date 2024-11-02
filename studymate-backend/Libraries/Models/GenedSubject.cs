namespace studymate_backend.Libraries.Models;

public class GenedSubject(
    string subject_id,
    string group_id
) : IBaseModel
{
    public string subject_id { get; set; } = subject_id;
    public string group_id { get; set; } = group_id;
}
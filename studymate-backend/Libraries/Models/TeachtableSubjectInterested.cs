namespace studymate_backend.Libraries.Models;

public class TeachtableSubjectInterested(
    TeachtableSubject? teachtable_subject,
    string user_id,
    int id = 0
) : IBaseModel
{
    public int id { get; set; } = id;
    public TeachtableSubject? teachtable_subject { get; set; } = teachtable_subject;
    public string user_id { get; set; }

}
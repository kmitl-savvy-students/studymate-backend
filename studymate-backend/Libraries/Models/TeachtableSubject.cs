namespace studymate_backend.Libraries.Models;

public class TeachtableSubject(
    int id,
    Teachtable? teachtable,
    string public_id,
    string subject_id,
    int interested,
    float rating
) : IBaseModel
{
    public int id { get; set; } = id;
    public Teachtable? teachtable { get; set; } = teachtable;
    public string public_id { get; set; } = public_id;
    public string subject_id { get; set; } = subject_id;
    public int interested { get; set; } = interested;
    public float rating { get; set; } = rating;
}
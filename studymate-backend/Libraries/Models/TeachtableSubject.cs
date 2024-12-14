namespace studymate_backend.Libraries.Models;

public class TeachtableSubject(
    Teachtable? teachtable,
    string subject_id,
    int interested,
    float rating,
    int id = 0
) : IBaseModel
{
    public int id { get; set; } = id;
    public Teachtable? teachtable { get; set; } = teachtable;
    public string subject_id { get; set; } = subject_id;
    public int interested { get; set; } = interested;
    public float rating { get; set; } = rating;
}
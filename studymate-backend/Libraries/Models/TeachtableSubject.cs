namespace studymate_backend.Libraries.Models;

public class TeachtableSubject(
    Teachtable? teachtable,
    string subject_id,
    int interested,
    float rating,
    int count_of_review,
    int id = 0
) : IBaseModel
{
    public int id { get; set; } = id;
    public Teachtable? teachtable { get; set; } = teachtable;
    public string subject_id { get; set; } = subject_id;
    public int interested { get; set; } = interested;
    public int count_of_review { get; set; } = count_of_review;
    public float rating { get; set; } = rating;
}
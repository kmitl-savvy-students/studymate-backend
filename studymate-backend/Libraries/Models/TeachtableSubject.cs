namespace studymate_backend.Libraries.Models;

public class TeachtableSubject(
    Teachtable? teachtable,
    string subjectId,
    int interested,
    float rating,
    int countOfReview,
    int id = 0
) : IBaseModel
{
    public int Id { get; set; } = id;
    public Teachtable? Teachtable { get; set; } = teachtable;
    public string SubjectId { get; set; } = subjectId;
    public int Interested { get; set; } = interested;
    public float Rating { get; set; } = rating;
    public int CountOfReview { get; set; } = countOfReview;
}
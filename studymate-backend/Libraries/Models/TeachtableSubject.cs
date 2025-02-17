namespace studymate_backend.Libraries.Models;

public class TeachtableSubject(
    Teachtable? teachtable, //tts_tt_id
    string subjectId, //tts_sbj_id
    int interested, //tts_int
    float rating, //tts_rat
    int countOfReview, //tts_cor
    int id = 0 //tts_id
) : IBaseModel
{
    public int Id { get; set; } = id;
    public Teachtable? Teachtable { get; set; } = teachtable;
    public string SubjectId { get; set; } = subjectId;
    public int Interested { get; set; } = interested;
    public float Rating { get; set; } = rating;
    public int CountOfReview { get; set; } = countOfReview;
}
namespace studymate_backend.Libraries.Models;

public class TeachtableSubjectReview(
    Teachtable? teachtable,
    string subjectId,
    string userId,
    string review,
    float rating,
    int like,
    DateOnly? created = null,
    int id = 0
) : IBaseModel
{
    public int Id { get; set; } = id;
    public Teachtable? Teachtable { get; set; } = teachtable;
    public string SubjectId { get; set; } = subjectId;
    public string UserId { get; set; } = userId;
    public string Review { get; set; } = review;
    public float Rating { get; set; } = rating;
    public int Like { get; set; } = like;
    public DateOnly Created { get; set; } = created ?? DateOnly.FromDateTime(DateTime.Now); // ถ้าไม่กำหนด ใช้วันที่ปัจจุบัน
    // Dynamic Property
    public string SubjectNameEn { get; set; } = "";

}
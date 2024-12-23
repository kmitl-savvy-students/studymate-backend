namespace studymate_backend.Libraries.Models;

public class TeachtableSubjectReview(
    TeachtableSubject? teachtable_subject,
    string user_id,
    string review,
    float rating,
    int like,
    // DateOnly created = default, // ใช้ default (ค่าเริ่มต้นของ DateOnly)
    DateOnly? created = null,
    int id = 0
) : IBaseModel
{
    public int id { get; set; } = id;
    public TeachtableSubject? teachtable_subject { get; set; } = teachtable_subject;
    public string user_id { get; set; } = user_id;
    public string review { get; set; } = review;
    public float rating { get; set; } = rating;
    public int like { get; set; } = like;
    // public DateOnly created { get; set; } = created == default ? DateOnly.FromDateTime(DateTime.Now) : created;
    public DateOnly created { get; set; } = created ?? DateOnly.FromDateTime(DateTime.Now); // ถ้าไม่กำหนด ใช้วันที่ปัจจุบัน

}
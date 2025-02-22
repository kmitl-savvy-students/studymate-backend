namespace studymate_backend.Libraries.Models;

public class SubjectReview(
    Teachtable? teachtable, //sbjr_tt_id
    string userId, //sbjr_user_id
    string review, //sbjr_rev
    float rating, //sbjr_rat
    int like, //sbjr_like
    string subjectId, //sbjr_sbj_id
    DateOnly? created = null, //sbjr_date_created
    int id = 0 //sbjr_id 
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
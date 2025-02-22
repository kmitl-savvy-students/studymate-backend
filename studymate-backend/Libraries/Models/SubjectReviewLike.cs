namespace studymate_backend.Libraries.Models;
public class SubjectReviewLike(
    string userId,
    SubjectReview subjectReview,
    int id = 0
) : IBaseModel
{
    public int Id { get; set; } = id;
    public SubjectReview? SubjectReview { get; set; } = subjectReview;
    public string UserId { get; set; } = userId;
}
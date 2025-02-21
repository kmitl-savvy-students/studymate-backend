namespace studymate_backend.Libraries.Models;
public class TeachtableSubjectReviewLike(
    string userId,
    TeachtableSubjectReview teachtableSubjectReview,
    int id = 0
) : IBaseModel
{
    public int Id { get; set; } = id;
    public TeachtableSubjectReview? TeachtableSubjectReview { get; set; } = teachtableSubjectReview;
    public string UserId { get; set; } = userId;
}
namespace studymate_backend.Libraries.Models;

public class TeachtableSubjectReviewLike(
    string user_id,
    TeachtableSubjectReview teachtable_subject_review_id,
    int id = 0
) : IBaseModel
{
    public int id { get; set; } = id;
    public TeachtableSubjectReview teachtable_subject_review_id { get; set; } = teachtable_subject_review_id;
    public string user_id { get; set; } = user_id;
}
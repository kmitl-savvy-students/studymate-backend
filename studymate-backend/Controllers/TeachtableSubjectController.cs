using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

[Route("teachtable-subject")]
[ApiController]
public class TeachtableSubjectController : ControllerBase
{
    [HttpGet("get/count-and-rating-review/{subjectId}")]
    public IActionResult GetReviewStats(string subjectId)
    {
        try
        {
            var stats = SdmTeachtableSubject.GetReviewStats(subjectId);
            if (stats == null)
            {
                return NotFound(new { message = "Subject not found or no reviews available." });
            }

            return Ok(new
            {
                subject_id = subjectId,
                count_of_review = stats.Value.countOfReview,
                average_rating = stats.Value.averageRating
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}
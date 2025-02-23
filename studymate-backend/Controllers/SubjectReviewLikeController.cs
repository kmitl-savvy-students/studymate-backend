using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/subject-review-like")]
public class SubjectReviewLikeController : ControllerBase
{

    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPost]
    public IActionResult Create([FromBody] TeachtableSubjectReviewLikeDto reviewLikeDto)
    {
        try
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = SdmSubjectReviewLike.GetUserInfoFromToken(token);
            
            SubjectReview existingReview = SdmSubjectReview.GetById(reviewLikeDto.TeachtableSubjectReviewId);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid or expired token." });
            }
            if (existingReview == null)
            {
                return NotFound(new { message = "Review not found."});
            }
            
            var alreadyLike = SdmSubjectReviewLike.GetByUserIdAndReviewId(user.Id, reviewLikeDto.TeachtableSubjectReviewId.ToString());

            if (alreadyLike != null)
            {
                return Conflict(new { message = "You like this review already"});
            }

            var reviewLike = new SubjectReviewLike(
                userId: user.Id,
                subjectReview: existingReview
            );

            SdmSubjectReviewLike.Insert(reviewLike);

            return Ok(new { message = "Like review successfully." });

        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Internal Server Error: {ex.Message}" });
        }
    }

    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpDelete("{teachtableSubjectReviewId}")]

    public IActionResult Delete(int teachtableSubjectReviewId)
    {
        try
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = SdmSubjectReviewLike.GetUserInfoFromToken(token);
            
            SubjectReview review = SdmSubjectReview.GetById(teachtableSubjectReviewId);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid or expired token." });
            }
            
            // ไม่พบรีวิว
            if (review == null)
            {
                return NotFound(new { message = "Subject review not found." });
            }

            
            var reviewLike = new SubjectReviewLike(
                userId: user.Id,
                subjectReview: review
            );

            Console.WriteLine($"{SdmSubjectReviewLike.GetByUserIdAndReviewId(user.Id, review.Id.ToString())}");

            if (SdmSubjectReviewLike.GetByUserIdAndReviewId(user.Id, review.Id.ToString()) == null)
            {
                return NotFound(new {message = "Like not found."});
            }

            SdmSubjectReviewLike.Delete(reviewLike);
            Console.WriteLine($"user_id = {user.Id}, review_id = {review.Id}");
            return Ok(new { message = "Unlike this review successfully." });

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500,
                new { message = "An error occurred while deleting the review.", error = ex.Message });
        }
    }

    [HttpGet("{teachtableSubjectReviewId}")]
    public IActionResult GetByTeachtableSubjectReviewId(int teachtableSubjectReviewId)
    {
        try
        {
            var review = SdmSubjectReview.GetById(teachtableSubjectReviewId);
            var reviewLike = SdmSubjectReviewLike.GetByReviewId(teachtableSubjectReviewId.ToString());

            if (review == null)
            {
                return NotFound(new { message = "Review not found." });
            }
            if (reviewLike == null)
            {
                return Ok(new List<SdmSubjectReviewLike>());
            }

            return Ok(reviewLike);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500,
                new { message = "An error occurred while deleting the review.", error = ex.Message });
        }
    }


}

public class TeachtableSubjectReviewLikeDto
{
    public int TeachtableSubjectReviewId { get; set; }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/teachtable-subject-review/like")]
public class TeachtableSubjectReviewLikeController : ControllerBase
{

    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPost]
    public IActionResult Create([FromBody] TeachtableSubjectReviewLikeDto reviewLikeDto)
    {
        try
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            TeachtableSubjectReview existingReview = SdmTeachtableSubjectReview.GetById(reviewLikeDto.TeachtableSubjectReviewId);

            if (existingReview == null)
            {
                return NotFound(new { message = "Review not found."});
            }

            var user = SdmTeachtableSubjectReviewLike.GetUserInfoFromToken(token);
            var alreadyLike = SdmTeachtableSubjectReviewLike.GetByUserIdAndReviewId(user.Id.ToString(), reviewLikeDto.TeachtableSubjectReviewId.ToString());

            if (alreadyLike != null)
            {
                return Conflict(new { message = "You like this review already"});
            }

            var reviewLike = new TeachtableSubjectReviewLike(
                userId: user.Id.ToString(),
                teachtableSubjectReview: existingReview
            );

            SdmTeachtableSubjectReviewLike.Insert(reviewLike);

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

            TeachtableSubjectReview review = SdmTeachtableSubjectReview.GetById(teachtableSubjectReviewId);

            // ไม่พบรีวิว
            if (review == null)
            {
                return NotFound(new { message = "Teachtable subject review not found." });
            }

            var user = SdmTeachtableSubjectReviewLike.GetUserInfoFromToken(token);
            var reviewLike = new TeachtableSubjectReviewLike(
                userId: user.Id.ToString(),
                teachtableSubjectReview: review
            );

            Console.WriteLine($"{SdmTeachtableSubjectReviewLike.GetByUserIdAndReviewId(user.Id.ToString(), review.Id.ToString())}");

            if (SdmTeachtableSubjectReviewLike.GetByUserIdAndReviewId(user.Id.ToString(), review.Id.ToString()) == null)
            {
                return NotFound(new {message = "Like not found."});
            }

            SdmTeachtableSubjectReviewLike.Delete(reviewLike);
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
            var review = SdmTeachtableSubjectReviewLike.GetByReviewId(teachtableSubjectReviewId.ToString());

            // ไม่พบรีวิว
            if (review == null)
            {
                return NotFound(new { message = "Teachtable subject review not found." });
            }

            return Ok(review);
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
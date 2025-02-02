using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Database;
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
            var alreadyLike = SdmTeachtableSubjectReviewLike.GetByUserIdAndReviewId(user.id, reviewLikeDto.TeachtableSubjectReviewId.ToString());

            if (alreadyLike != null)
            {
                return Conflict(new { message = "You like this review already"});
            }
                
            var reviewLike = new TeachtableSubjectReviewLike(
                user_id: user.id,
                teachtable_subject_review: existingReview
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
                user_id: user.id,
                teachtable_subject_review: review
            );
            
            Console.WriteLine($"{SdmTeachtableSubjectReviewLike.GetByUserIdAndReviewId(user.id, review.id.ToString())}");

            if (SdmTeachtableSubjectReviewLike.GetByUserIdAndReviewId(user.id, review.id.ToString()) == null)
            {
                return NotFound(new {message = "Like not found."});
            }
            
            SdmTeachtableSubjectReviewLike.Delete(reviewLike);
            Console.WriteLine($"user_id = {user.id}, review_id = {review.id}");
            return Ok(new { message = "Unlike this review successfully." });

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
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
                return NotFound(new { message = "Teachtable subject review not found."});
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
}

public class TeachtableSubjectReviewLikeDto
{
    public int TeachtableSubjectReviewId { get; set; }
}
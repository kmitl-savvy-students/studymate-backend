using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/teachtable-subject-review")]
public class TeachtableSubjectReviewController : ControllerBase
{
    
    [HttpGet]
    public IActionResult GetAll()
    {
        var reviews = SdmTeachtableSubjectReview.GetAll();

        if (reviews.Count == 0)
            return Ok(reviews);
        return Ok(reviews);
    }
    
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPost]
    public IActionResult Create([FromBody] TeachtableSubjectReviewDto reviewDto)
    {
        try
        {
            SdmTeachtableSubjectReview.CreateReview(
                studentId: reviewDto.StudentId,
                year: reviewDto.Year,
                term: reviewDto.Term,
                subjectId: reviewDto.SubjectId,
                review: reviewDto.Review,
                rating: reviewDto.Rating
            );

            return Ok(new { message = "Review created successfully." });
        }
        catch (InvalidOperationException ex) // ตรวจจับรีวิวที่มีอยู่แล้ว
        {
            return Conflict(new { message = ex.Message });  // ส่ง 409 Conflict กลับไป
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new { message = "An error occurred while creating the review.", error = ex.Message });
        }
    }

    [HttpGet("{subjectId}/{studentId}")]
    public IActionResult GetBySubjectAndStudent(string subjectId, string studentId)
    {
        try
        {
            var review = SdmTeachtableSubjectReview.GetBySubjectAndStudent(subjectId, studentId);
            if (review == null)
            {
                return Ok(new object[] { });
            }

            return Ok(review);
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new { message = "An error occurred while fetching the review.", error = ex.Message });
        }
    }
    
    [HttpGet("{subjectId}")]
    public IActionResult GetBySubject(string subjectId)
    {
        try
        {
            var review = SdmTeachtableSubjectReview.GetBySubject(subjectId);
            if (review == null|| review.Count == 0)
            {
                return Ok(new object[] { });
            }

            return Ok(review);
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new { message = "An error occurred while fetching the review.", error = ex.Message });
        }
    }

    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPatch("update")]
    public IActionResult Update([FromBody] TeachtableSubjectReviewDto reviewDto)
    {
        try
        {
            SdmTeachtableSubjectReview.Update(
                studentId: reviewDto.StudentId,
                year: reviewDto.Year,
                term: reviewDto.Term,
                subjectId: reviewDto.SubjectId,
                review: reviewDto.Review,
                rating: reviewDto.Rating
            );

            return Ok(new { message = "Review updated successfully." });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = "An error occurred while updating the review.", error = ex.Message });
        }
    }
    
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpDelete("{subjectId}/{studentId}")]
    public IActionResult Delete(string subjectId, string studentId)
    {
        try
        {
            var review = SdmTeachtableSubjectReview.GetBySubjectAndStudent(subjectId, studentId);
            if (review == null)
            {
                return NotFound(new { message = "Review not found." });
            }
            SdmTeachtableSubjectReview.Delete(subjectId, studentId);
            return Ok(new { message = "Review deleted successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new { message = "An error occurred while deleting the review.", error = ex.Message });
        }
    }
}

public class TeachtableSubjectReviewDto
{
    public string StudentId { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Term { get; set; }
    public string SubjectId { get; set; } = string.Empty;
    public string Review { get; set; } = string.Empty;
    public float Rating { get; set; } 
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/teachtablesubjectreview")]
public class TeachtableSubjectReviewController : ControllerBase
{
    // ดึงข้อมูลทั้งหมด
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpGet]
    public IActionResult GetAll()
    {
        try
        {
            var reviews = SdmTeachtableSubjectReview.GetAll();
            return Ok(reviews);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching reviews.", error = ex.Message });
        }
    }

    // ดึงข้อมูลตาม ID
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        try
        {
            var review = SdmTeachtableSubjectReview.GetById(id);
            if (review == null)
            {
                return NotFound(new { message = "Review not found." });
            }
            return Ok(review);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching the review.", error = ex.Message });
        }
    }

    // เพิ่มรีวิวใหม่
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
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the review.", error = ex.Message });
        }
    }

    // อัปเดตรีวิว
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] TeachtableSubjectReviewDto reviewDto)
    {
        try
        {
            var review = SdmTeachtableSubjectReview.GetById(id);
            if (review == null)
            {
                return NotFound(new { message = "Review not found." });
            }

            // อัปเดตค่าของ Review
            review.review = reviewDto.Review;
            review.rating = reviewDto.Rating;

            SdmTeachtableSubjectReview.Update(review);

            return Ok(new { message = "Review updated successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the review.", error = ex.Message });
        }
    }

    // ลบรีวิว
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        try
        {
            var review = SdmTeachtableSubjectReview.GetById(id);
            if (review == null)
            {
                return NotFound(new { message = "Review not found." });
            }

            SdmTeachtableSubjectReview.Delete(review);
            return Ok(new { message = "Review deleted successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the review.", error = ex.Message });
        }
    }
}

// DTO สำหรับรับข้อมูลจาก Request
public class TeachtableSubjectReviewDto
{
    public string StudentId { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Term { get; set; }
    public string SubjectId { get; set; } = string.Empty;
    public string Review { get; set; } = string.Empty;
    public float Rating { get; set; }
}

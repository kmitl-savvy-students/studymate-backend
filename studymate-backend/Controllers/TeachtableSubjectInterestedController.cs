using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/teachtable-subject-interested")]
public class TeachtableSubjectInterestedController : ControllerBase
{
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPost]
    public IActionResult Create([FromBody] TeachtableSubjectInterestedDto interestedDto)
    {
        try
        {
            SdmTeachtableSubjectInterested.CreateInterested(
                studentId: interestedDto.StudentId,
                year: interestedDto.Year,
                term: interestedDto.Term,
                subjectId: interestedDto.SubjectId
            );

            return Ok(new { message = "Review created successfully." });
        }
        catch (InvalidOperationException ex) // ตรวจจับรีวิวที่มีอยู่แล้ว
        {
            return Conflict(new { message = ex.Message });  // ส่ง 409 Conflict กลับไป
        }
        catch (Exception ex)
        {
            Console.WriteLine(interestedDto.StudentId);
            return StatusCode(500,
                new { message = "An error occurred while creating the review.", error = ex.Message });
        }
    }
    
}
public class TeachtableSubjectInterestedDto
{
    public string StudentId { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Term { get; set; }
    public string SubjectId { get; set; } = string.Empty;
}
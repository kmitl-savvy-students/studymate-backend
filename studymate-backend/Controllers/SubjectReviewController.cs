using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
namespace studymate_backend.Controllers;

[ApiController]
[Route("api/subject-review")]
public class SubjectReviewController : ControllerBase
{
    
    [HttpGet]
    public IActionResult GetAll()
    {
        var reviews = SdmSubjectReview.GetAll();
        
        if (reviews.Count == 0)
            return NotFound(new { message = "Reviews not found." });
        return Ok(reviews);
    }
    
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPost]
    public IActionResult Create([FromBody] TeachtableSubjectReviewDto reviewDto)
    {
        try
        {
            // ดึง Token จาก Header
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    
            // ดึงข้อมูลผู้ใช้จาก Token
            var user = SdmSubjectReview.GetUserInfoFromToken(token);
            if (user.Id != reviewDto.StudentId || user == null)
            {
                return Unauthorized(new { message = "Invalid or expired token." });
            }
            
            SdmSubjectReview.CreateReview(
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
            var review = SdmSubjectReview.GetBySubjectAndStudent(subjectId, studentId);
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
            var review = SdmSubjectReview.GetBySubjectId(subjectId);
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
    [HttpDelete("{subjectId}/{studentId}")]
    public IActionResult Delete(string subjectId, string studentId)
    {
        try
        {
            // ดึง Token จาก Header
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    
            // ดึงข้อมูลผู้ใช้จาก Token
            var user = SdmSubjectReview.GetUserInfoFromToken(token);
            if (user.Id.ToString() != studentId || user == null)
            {
                return Unauthorized(new { message = "Invalid or expired token." });
            }
            
            var review = SdmSubjectReview.GetBySubjectAndStudent(subjectId, studentId);
            if (review == null)
            {
                return NotFound(new { message = "Review not found." });
            }

            SdmSubjectReview.Delete(subjectId, studentId);

            // ตรวจสอบว่าข้อมูลถูกลบจริงหรือไม่
            var remainingReview = SdmSubjectReview.GetBySubjectAndStudent(subjectId, studentId);
            if (remainingReview == null)
            {
                return Ok(new { message = "Review deleted successfully." });
            }
            else
            {
                return StatusCode(500, new { message = "Failed to delete the review." });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new { message = "An error occurred while deleting the review.", error = ex.Message });
        }
    }
    
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpGet("current")]
    public async Task<IActionResult> GetLatestSubjects()
    {
        try
        {
            // ดึง Token จาก Header
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    
            // ดึงข้อมูลผู้ใช้จาก Token
            var user = SdmSubjectReview.GetUserInfoFromToken(token);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid or expired token." });
            }
    
            Console.WriteLine($"[User Info] UserId: {user.Id}, Year: {user.Curriculum?.Year}");
    
            // ตรวจสอบว่า User มี Curriculum หรือไม่
            if (user.Curriculum == null)
            {
                return NotFound(new { message = "You must login and select curriculum." });
            }
    
            var publicId = user.Curriculum.Program.KmitlId;
    
            // เรียกใช้ฟังก์ชันดึงข้อมูลล่าสุด
            var allSubjects = await SdmSubjectReview.GetAllSubjectInFacultyAndGened(user);
            
            // ดึงรีวิวที่เกี่ยวข้องกับ allSubjects
            var reviews = SdmSubjectReview.GetReviewsBySubjects(allSubjects);
    
            return Ok(reviews);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Controller Error] {ex.Message}");
            return StatusCode(500, new { message = "Error occurred while fetching data.", error = ex.Message });
        }
    }
    
    [HttpGet("average/{subjectId}")]
    public ActionResult<double> GetRating(string subjectId)
    {
        try
        {
            double review = SdmSubjectReview.GetAverageRatingOfReview(subjectId);
            return Ok(review);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching the review.", error = ex.Message });
        }
    }
    
    [HttpGet("count/{subjectId}")]
    public ActionResult<int> GetCount(string subjectId)
    {
        try
        {
            double review = SdmSubjectReview.GetCountOfReview(subjectId);
            return Ok(review);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching the review.", error = ex.Message });
        }
    }

    
}

public class TeachtableSubjectReviewDto
{
    public int StudentId { get; set; }
    public int Year { get; set; }
    public int Term { get; set; }
    public string SubjectId { get; set; } = string.Empty;
    public string Review { get; set; } = string.Empty;
    public float Rating { get; set; } 
}
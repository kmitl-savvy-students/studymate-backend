using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

[Route("api/teachtable-subject")]
[ApiController]
public class TeachtableSubjectController : ControllerBase
{
    [HttpGet("get/{subjectId}")]
    public IActionResult GetBySubject(string subjectId)
    {
        try
        {
            var teachtableSubject = SdmTeachtableSubject.GetBySubject(subjectId);
            if (teachtableSubject == null)
            {
                return NotFound(new {message = "This subjectId does not exist in TeachtableSubject."});
            }

            return Ok(teachtableSubject);

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}
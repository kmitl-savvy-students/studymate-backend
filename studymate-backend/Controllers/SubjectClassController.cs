using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/subject-class")]
public class SubjectClassController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("get-by-class")]
    public ActionResult<IEnumerable<SubjectClass>> GetAllBy(
        [FromQuery(Name="academic_year")] string academicYear,
        [FromQuery(Name="academic_term")] string academicTerm, 
        [FromQuery] string year, 
        [FromQuery] string program
    ) {
        return Ok(SdmSubjectClass.GetAllBy(
            SdmTeachtable.GetBy(
                Convert.ToInt32(academicYear), 
                Convert.ToInt32(academicTerm)
                ),
            SdmProgram.GetBy(Convert.ToInt32(program)),
            year
            )
        ); 
    }
}
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
    public async Task<ActionResult<IEnumerable<SubjectClass>>> GetAllBy(
        [FromQuery(Name = "academic_year")] int academicYear,
        [FromQuery(Name = "academic_term")] int academicTerm,
        [FromQuery] string year,
        [FromQuery] int program
    )
    {
        var teachtable = SdmTeachtable.GetBy(academicYear, academicTerm);
        var programData = SdmProgram.GetBy(program);

        if (teachtable == null || programData == null)
            return BadRequest("Invalid academic year, term, or program.");

        var subjectClasses = await SdmSubjectClass.GetAllBy(teachtable, programData, year);

        return Ok(subjectClasses);
    }
    [AllowAnonymous]
    [HttpGet("get-by-subject-id")]
    public async Task<ActionResult<SubjectClass>> GetAllBy(
        [FromQuery(Name = "academic_year")] int academicYear,
        [FromQuery(Name = "academic_term")] int academicTerm,
        [FromQuery] int program,
        [FromQuery] string subjectId,
        [FromQuery] string section
    )
    {
        var teachtable = SdmTeachtable.GetBy(academicYear, academicTerm);
        var programData = SdmProgram.GetBy(program);

        if (teachtable == null || programData == null)
            return BadRequest("Invalid academic year, term, or program.");

        var subjectClass = await SdmSubjectClass.GetBy(teachtable, programData, subjectId, section);

        return Ok(subjectClass);
    }
    [AllowAnonymous]
    [HttpGet("get-subject-availability")]
    public async Task<ActionResult<string>> GetBy(
        [FromQuery(Name = "academic_year")] int academicYear,
        [FromQuery(Name = "academic_term")] int academicTerm,
        [FromQuery] int program,
        [FromQuery] string subjectId
    )
    {
        var teachtable = SdmTeachtable.GetBy(academicYear, academicTerm);
        var programData = SdmProgram.GetBy(program);

        if (teachtable == null || programData == null)
            return BadRequest("Invalid academic year, term, or program.");

        var result = await SdmSubjectClass.GetBy(teachtable, programData, subjectId);
        return Ok(result ? "true" : "false");
    }
}
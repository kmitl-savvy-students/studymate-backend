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
        [FromQuery] int curriculumId
    )
    {
        var teachtable = SdmTeachtable.GetBy(academicYear, academicTerm);
        var curriculum = curriculumId == 0 ? new Curriculum(0, null, 0, "x", "x", null) : SdmCurriculum.GetBy(curriculumId);

        if (teachtable == null || curriculum == null)
            return BadRequest("Invalid academic year, term, or curriculum.");

        var subjectClasses = await SdmSubjectClass.GetAllBy(teachtable, curriculum, year);

        return Ok(subjectClasses);
    }
    [AllowAnonymous]
    [HttpGet("get-by-subject-id")]
    public async Task<ActionResult<SubjectClass>> GetAllBy(
        [FromQuery(Name = "academic_year")] int academicYear,
        [FromQuery(Name = "academic_term")] int academicTerm,
        [FromQuery] int curriculumId,
        [FromQuery] string subjectId,
        [FromQuery] string section
    )
    {
        var teachtable = SdmTeachtable.GetBy(academicYear, academicTerm);
        var curriculum = SdmCurriculum.GetBy(curriculumId);

        if (teachtable == null || curriculum == null)
            return BadRequest("Invalid academic year, term, or curriculum.");

        var subjectClass = await SdmSubjectClass.GetBy(teachtable, curriculum, subjectId, section);

        return Ok(subjectClass);
    }
    [AllowAnonymous]
    [HttpGet("get-subject-availability")]
    public async Task<ActionResult<string>> GetBy(
        [FromQuery(Name = "academic_year")] int academicYear,
        [FromQuery(Name = "academic_term")] int academicTerm,
        [FromQuery] int curriculumId,
        [FromQuery] string subjectId
    )
    {
        var teachtable = SdmTeachtable.GetBy(academicYear, academicTerm);
        var curriculum = SdmCurriculum.GetBy(curriculumId);

        if (teachtable == null || curriculum == null)
            return BadRequest("Invalid academic year, term, or curriculum.");

        var result = await SdmSubjectClass.GetBy(teachtable, curriculum, subjectId);
        return Ok(result ? "true" : "false");
    }
    [AllowAnonymous]
    [HttpGet("get-subject")]
    public async Task<ActionResult<Subject?>> GetBy(
        [FromQuery] string subjectId
    )
    {
        var result = await SdmSubjectClass.GetBy(subjectId);
        return Ok(result);
    }
}
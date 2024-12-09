using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Helper;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/curriculum-teachtable-subject")]
public class CurriculumTeachtableController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("{year}/{semester}/{faculty}/{department}/{curriculum}/{classYear}/{curriculumYear}/{uniqueId}")]
    public async Task<IActionResult> Get(
        [FromRoute] int year,
        [FromRoute] int semester,
        [FromRoute] string faculty,
        [FromRoute] string department,
        [FromRoute] string curriculum,
        [FromRoute] int classYear,
        [FromRoute] string curriculumYear,
        [FromRoute] string uniqueId) // เพิ่ม uniqueId)
    {
        if (curriculumYear != "2560" && curriculumYear != "2564")
        {
            return BadRequest(new { message = "curriculumYear must be either 2560 or 2564." });
        }

        if (!SdmNumber.IsAcademicYear(year) || !SdmNumber.IsAcademicTerm(semester) || !SdmNumber.IsClassYear(classYear))
        {
            return BadRequest(new { message = "Invalid request data." });
        }

        try
        {
            var filteredData = await SdmCurriculumTeachtable.FetchFilteredTeachTableData(
                year, semester, faculty, department, curriculum, classYear, curriculumYear, uniqueId);

            return Ok(filteredData);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}

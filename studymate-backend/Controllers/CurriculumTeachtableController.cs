using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/curriculum-teachtable-subject")]
public class CurriculumTeachtableController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("{year}/{semester}/{faculty}/{department}/{curriculum}/{classYear}")]
    public async Task<IActionResult> Get(
        [FromRoute] int year,
        [FromRoute] int semester,
        [FromRoute] string faculty,
        [FromRoute] string department,
        [FromRoute] string curriculum,
        [FromRoute] int classYear)
    {
        try
        {
            var filteredData = await SdmCurriculumTeachtable.FetchFilteredTeachTableData(
                year, semester, faculty, department, curriculum, classYear);
    
            return Ok(filteredData);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
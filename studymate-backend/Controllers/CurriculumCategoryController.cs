using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/curriculum-category")]
public class CurriculumCategoryController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("query/{uniqueId}/{year}")]
    public ActionResult<List<CurriculumCategory>> QueryBy(string uniqueId, string year)
    {
        var curriculumCategories = SdmCurriculumCategory.QueryBy(uniqueId, year);
        return Ok(curriculumCategories);
    }
}
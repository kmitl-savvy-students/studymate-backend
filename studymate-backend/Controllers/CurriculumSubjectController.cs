using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/curriculum-subject")]
public class CurriculumSubjectController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("get")]
    public ActionResult<IEnumerable<CurriculumSubject>> Get()
    {
        var curriculumSubjects = SdmCurriculumSubject.GetAll();

        if (curriculumSubjects.Count == 0)
            return NotFound(new { message = "Curriculum subject not found." });
        return Ok(curriculumSubjects);
    }

    [AllowAnonymous]
    [HttpGet("get/{subjectId}/{uniqueId}/{year}")]
    public ActionResult<CurriculumSubject> Get(string subjectId, string uniqueId, string year)
    {
        var curriculumSubject = SdmCurriculumSubject.GetBy(subjectId, uniqueId, year);

        if (curriculumSubject == null)
            return NotFound(new { message = "Curriculum subject not found." });
        return Ok(curriculumSubject);
    }

    [AllowAnonymous]
    [HttpGet("get/{categoryId}/{groupId}/{subgroupId}/{uniqueId}/{year}")]
    public ActionResult<IEnumerable<SdmCurriculumSubject>> GetAllBy(int categoryId, int groupId, int subgroupId, string uniqueId, string year)
    {
        var curriculumSubjects = SdmCurriculumSubject.GetAllBy(categoryId, groupId, subgroupId, uniqueId, year);

        if (curriculumSubjects.Count == 0)
            return NotFound(new { message = "Curriculum subject not found." });
        return Ok(curriculumSubjects);
    }
    
    [AllowAnonymous]
    [HttpGet("query/{uniqueId}/{year}/{categoryId}/{groupId}/{subgroupId}")]
    public ActionResult<List<CurriculumSubject>> QueryBy(string uniqueId, string year, string categoryId, string groupId, string subgroupId)
    {
        var curriculumSubjects = SdmCurriculumSubject.QueryBy(uniqueId, year, categoryId, groupId, subgroupId);
        return Ok(curriculumSubjects);
    }
}

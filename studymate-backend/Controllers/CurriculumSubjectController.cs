using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/curriculum-subject/get")]
public class CurriculumSubjectController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public ActionResult<IEnumerable<CurriculumSubject>> Get()
    {
        var curriculumSubjects = SdmCurriculumSubject.GetAll();

        if (curriculumSubjects.Count == 0)
            return NotFound(new { message = "Curriculum subject not found." });
        return Ok(curriculumSubjects);
    }

    [AllowAnonymous]
    [HttpGet("{subjectId}/{uniqueId}/{year}")]
    public ActionResult<CurriculumSubject> Get(string subjectId, string uniqueId, string year)
    {
        var curriculumSubject = SdmCurriculumSubject.GetBy(subjectId, uniqueId, year);

        if (curriculumSubject == null)
            return NotFound(new { message = "Curriculum subject not found." });
        return Ok(curriculumSubject);
    }

    [AllowAnonymous]
    [HttpGet("{categoryId}/{groupId}/{subgroupId}/{uniqueId}/{year}")]
    public ActionResult<IEnumerable<SdmCurriculumSubject>> GetAllBy(int categoryId, int groupId, int subgroupId, string uniqueId, string year)
    {
        var curriculumSubjects = SdmCurriculumSubject.GetAllBy(categoryId, groupId, subgroupId, uniqueId, year);

        if (curriculumSubjects.Count == 0)
            return NotFound(new { message = "Curriculum subject not found." });
        return Ok(curriculumSubjects);
    }
}

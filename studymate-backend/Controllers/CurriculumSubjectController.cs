using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/curriculumsubject/get")]
public class CurriculumSubjectController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<CurriculumSubject>> Get()
    {
        var curriculumSubjects = SdmCurriculumSubject.getAll();

        if (curriculumSubjects.Count == 0)
            return NotFound("Curriculum subject not found.");
        return Ok(curriculumSubjects);
    }

    [HttpGet("{subject_id:string}")]
    public ActionResult<CurriculumSubject> Get(string subject_id)
    {
        var curriculumSubjects = SdmCurriculumSubject.getById(subject_id);

        if (curriculumSubjects == null)
            return NotFound("Curriculum subject not found.");
        return Ok(curriculumSubjects);
    }
}
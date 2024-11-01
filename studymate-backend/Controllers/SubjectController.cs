using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/subject/get")]
public class SubjectController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Subject>> Get()
    {
        var subjects = SdmSubject.getAll();

        if (subjects.Count == 0)
            return NotFound("Subject not found.");
        return Ok(subjects);
    }

    [HttpGet("{subject_id}")]
    public ActionResult<Subject> Get(string subject_id)
    {
        var subjects = SdmSubject.getById(subject_id);
    
        if (subjects == null)
            return NotFound("Subject not found.");
        return Ok(subjects);
    }
}
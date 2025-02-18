using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/subject/get")]
public class SubjectController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public ActionResult<IEnumerable<Subject>> Get()
    {
        var subjects = SdmSubject.GetAll();

        if (subjects.Count == 0)
            return NotFound(new { message = "Subject not found." });
        return Ok(subjects);
    }

    [AllowAnonymous]
    [HttpGet("{subjectId}")]
    public ActionResult<Subject> Get(string subjectId)
    {
        var subjects = SdmSubject.GetBy(subjectId);

        if (subjects == null)
            return NotFound(new { message = "Subject not found." });
        return Ok(subjects);
    }
}
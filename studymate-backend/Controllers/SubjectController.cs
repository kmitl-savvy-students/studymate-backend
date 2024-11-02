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
            return NotFound("Subject not found.");
        return Ok(subjects);
    }

    [AllowAnonymous]
    [HttpGet("{subject_id}")]
    public ActionResult<Subject> Get(string subject_id)
    {
        var subjects = SdmSubject.GetById(subject_id);

        if (subjects == null)
            return NotFound("Subject not found.");
        return Ok(subjects);
    }
}

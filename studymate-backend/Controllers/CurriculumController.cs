using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/curriculum/get")]
public class CurriculumController : ControllerBase
{
    #region [GET] Curriculum
    [AllowAnonymous]
    [HttpGet]
    public ActionResult<IEnumerable<Curriculum>> GetAll()
    {
        return Ok(SdmCurriculum.GetAll());
    }
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public ActionResult<Curriculum> GetBy(int id)
    {
        return Ok(SdmCurriculum.GetBy(id));
    }
    #endregion
}
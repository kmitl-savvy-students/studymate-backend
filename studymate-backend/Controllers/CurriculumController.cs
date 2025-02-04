using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/curriculum")]
public class CurriculumController : ControllerBase
{
    #region [PUT] Update
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPut("update")]
    public ActionResult<Curriculum> Update(Curriculum curriculum)
    {
        SdmCurriculum.UpdateBy(curriculum);
        return Ok();
    }
    #endregion
    #region [POST] Create
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPost("create")]
    public ActionResult<Curriculum> Create(Curriculum curriculum)
    {
        SdmCurriculum.Insert(curriculum);
        return Ok();
    }
    #endregion
    #region [GET] Get
    [AllowAnonymous]
    [HttpGet("get")]
    public ActionResult<IEnumerable<Curriculum>> GetAll()
    {
        return Ok(SdmCurriculum.GetAll());
    }
    [AllowAnonymous]
    [HttpGet("get/{id:int}")]
    public ActionResult<Curriculum> GetBy(int id)
    {
        return Ok(SdmCurriculum.GetBy(id));
    }
    [AllowAnonymous]
    [HttpGet("get-by-program/{programId:int}")]
    public ActionResult<IEnumerable<Curriculum>> GetAllBy(int programId)
    {
        return Ok(SdmCurriculum.GetAllBy(programId));
    }
    #endregion
}
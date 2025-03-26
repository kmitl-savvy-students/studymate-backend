using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/program")]
public class ProgramController : ControllerBase
{
    #region [POST] Create
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPost("create")]
    public ActionResult<Libraries.Models.Program> Create(Libraries.Models.Program program)
    {
        SdmProgram.Insert(program);
        return Ok();
    }
    #endregion
    #region [PUT] Update
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPut("update")]
    public ActionResult<Libraries.Models.Program> Update(Libraries.Models.Program program)
    {
        SdmProgram.UpdateBy(program);
        return Ok();
    }
    #endregion
    #region [GET] Get
    [AllowAnonymous]
    [HttpGet("get")]
    public ActionResult<IEnumerable<Libraries.Models.Program>> GetAll()
    {
        return Ok(SdmProgram.GetAll());
    }
    [AllowAnonymous]
    [HttpGet("get/{id:int}")]
    public ActionResult<Libraries.Models.Program> GetBy(int id)
    {
        return Ok(SdmProgram.GetBy(id));
    }
    [AllowAnonymous]
    [HttpGet("get-by-department/{departmentId:int}")]
    public ActionResult<IEnumerable<Libraries.Models.Program>> GetAllBy(int departmentId)
    {
        return Ok(SdmProgram.GetAllBy(departmentId));
    }
    #endregion
}
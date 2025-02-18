using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/faculty")]
public class FacultyController : ControllerBase
{
    #region [POST] Create
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPost("create")]
    public ActionResult<Faculty> Create(Faculty faculty)
    {
        SdmFaculty.Insert(faculty);
        return Ok();
    }
    #endregion
    #region [PUT] Update
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPut("update")]
    public ActionResult<Faculty> Update(Faculty faculty)
    {
        SdmFaculty.UpdateBy(faculty);
        return Ok();
    }
    #endregion
    #region [GET] Get
    [AllowAnonymous]
    [HttpGet("get")]
    public ActionResult<IEnumerable<Faculty>> GetAll()
    {
        return Ok(SdmFaculty.GetAll());
    }
    [AllowAnonymous]
    [HttpGet("get/{id:int}")]
    public ActionResult<Faculty> GetBy(int id)
    {
        return Ok(SdmFaculty.GetBy(id));
    }
    #endregion
}
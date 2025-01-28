using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/curriculum-type")]
public class CurriculumTypeController : ControllerBase
{
    #region [POST] Create Curriculum Type
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPost("create")]
    public ActionResult<CurriculumType> Create(CurriculumType curriculumType)
    {
        SdmCurriculumType.Insert(curriculumType);
        return Ok();
    }
    #endregion
    #region [PUT] Update Curriculum Type
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPut("update")]
    public ActionResult<CurriculumType> Update(CurriculumType curriculumType)
    {
        SdmCurriculumType.UpdateBy(curriculumType);
        return Ok();
    }
    #endregion
    #region [GET] Get Curriculum Type
    [AllowAnonymous]
    [HttpGet("get")]
    public ActionResult<IEnumerable<CurriculumType>> GetAll()
    {
        return Ok(SdmCurriculumType.GetAll());
    }
    [AllowAnonymous]
    [HttpGet("get-by-department/{id:int}")]
    public ActionResult<IEnumerable<CurriculumType>> GetAllBy(int departmentId)
    {
        return Ok(SdmCurriculumType.GetAllBy(departmentId));
    }
    #endregion
}
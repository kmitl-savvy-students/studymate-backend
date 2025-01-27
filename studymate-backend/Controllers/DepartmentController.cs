using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/department")]
public class DepartmentController : ControllerBase
{
    #region [POST] Create Department
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPost("create")]
    public ActionResult<Department> Create(Department department)
    {
        SdmDepartment.Insert(department);
        return Ok();
    }
    #endregion
    #region [PATCH] Update Department
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPatch("update")]
    public ActionResult<Department> Update(Department department)
    {
        SdmDepartment.UpdateBy(department);
        return Ok();
    }
    #endregion
    #region [GET] Department
    [AllowAnonymous]
    [HttpGet("get")]
    public ActionResult<IEnumerable<Department>> GetAll()
    {
        return Ok(SdmDepartment.GetAll());
    }
    [AllowAnonymous]
    [HttpGet("get-by-faculty/{facultyId:int}")]
    public ActionResult<IEnumerable<Department>> GetAllBy(int facultyId)
    {
        return Ok(SdmDepartment.GetAllBy(facultyId));
    }
    [AllowAnonymous]
    [HttpGet("get/{id:int}")]
    public ActionResult<Department> GetBy(int id)
    {
        return Ok(SdmDepartment.GetBy(id));
    }
    #endregion
}
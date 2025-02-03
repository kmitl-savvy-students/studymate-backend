using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/curriculum-group")]
public class CurriculumGroupController : ControllerBase
{
    #region [GET] Get Curriculum Group
    [AllowAnonymous]
    [HttpGet("get-by-parent/{parentId:int}")]
    public ActionResult<IEnumerable<CurriculumGroup>> GetAllBy(int parentId)
    {
        return Ok(SdmCurriculumGroup.GetAllBy(parentId));
    }
    #endregion
    #region [PUT] Update Curriculum Group
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPut("update")]
    public ActionResult<Curriculum> Update(CurriculumGroup curriculumGroup)
    {
        SdmCurriculumGroup.UpdateBy(curriculumGroup);
        return Ok();
    }
    #endregion
    #region [POST] Create Curriculum Group
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPost("create")]
    public ActionResult<CurriculumGroup> Create(DtoCreate curriculumGroup)
    {
        return Ok(SdmCurriculumGroup.Insert(new CurriculumGroup(
            curriculumGroup.Id,
            curriculumGroup.ParentId,
            curriculumGroup.Type,
            curriculumGroup.Name,
            []
        )));
    }

    public class DtoCreate
    {
        public required int Id { get; init; } = -1;
        public required int ParentId { get; init; } = -1;
        public required string Type { get; init; } = string.Empty;
        public required string Name { get; init; } = string.Empty;
    }
    #endregion
}
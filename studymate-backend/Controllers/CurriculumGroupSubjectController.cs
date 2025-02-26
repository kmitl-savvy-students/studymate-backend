using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/curriculum-group-subject")]
public class CurriculumGroupSubjectController : ControllerBase
{
    #region [DELETE] Delete
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpDelete("delete/{id:int}")]
    public ActionResult Delete(int id)
    {
        SdmCurriculumGroupSubject.DeleteBy(id);
        return Ok();
    }
    #endregion
    #region [GET] Get
    [AllowAnonymous]
    [HttpGet("get-by-curriculum-group/{curriculumGroupId:int}")]
    public ActionResult<List<CurriculumGroupSubject>> GetBy(int curriculumGroupId)
    {
        return Ok(SdmCurriculumGroupSubject.GetAllBy(curriculumGroupId));
    }
    #endregion
    #region [POST] Create
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPost("create")]
    public async Task<ActionResult<DtoCreateCurriculumGroupSubject>> Create(DtoCreateCurriculumGroupSubject curriculumGroupSubject)
    {
        var curriculumGroup = SdmCurriculumGroup.GetBy(curriculumGroupSubject.CurriculumGroupId);
        var subjectIds = curriculumGroupSubject.SubjectString.Split(',').ToList();
        foreach (var subjectId in subjectIds)
        {
            var mySubjectId = subjectId.Trim()[..8];

            if (mySubjectId == string.Empty) continue;
            if (SdmCurriculumGroupSubject.GetBy(curriculumGroupSubject.CurriculumGroupId, mySubjectId) != null) continue;
            var trySubject = SdmSubject.GetBy(subjectId);
            if (trySubject == null)
            {
                trySubject = await SdmSubjectClass.GetBy(subjectId);
                if (trySubject == null)
                {
                    Console.WriteLine($"[WARN] Subject ID: {subjectId} not found even from the api. Skipping...");
                    continue;
                }
            }

            SdmCurriculumGroupSubject.Insert(new CurriculumGroupSubject(
                -1,
                curriculumGroup,
                SdmSubject.GetBy(mySubjectId)
            ));
        }

        return Ok();
    }

    public class DtoCreateCurriculumGroupSubject
    {
        public required int CurriculumGroupId { get; init; } = -1;
        public required string SubjectString { get; init; } = string.Empty;
    }
    #endregion
}
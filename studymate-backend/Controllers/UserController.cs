using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    #region [POST] Update
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPut("update/data")]
    public ActionResult<User> Update([FromBody] DtoUpdateUser user)
    {
        var existingUser = SdmUser.GetBy(user.Id);
        if (existingUser == null)
            return NotFound();

        if (user.NickName.Trim() != "")
            existingUser.Nickname = user.NickName;
        if (user.FirstName.Trim() != "")
            existingUser.Firstname = user.FirstName;
        if (user.LastName.Trim() != "")
            existingUser.Lastname = user.LastName;

        SdmUser.UpdateBy(existingUser);

        return Ok(user);
    }

    public class DtoUpdateUser
    {
        public required int Id { get; init; } = -1;
        public required string NickName { get; init; } = string.Empty;
        public required string FirstName { get; init; } = string.Empty;
        public required string LastName { get; init; } = string.Empty;
    }

    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPut("update/curriculum")]
    public ActionResult<User> Update([FromBody] DtoUpdateUserCurriculum user)
    {
        var existingUser = SdmUser.GetBy(user.Id);
        if (existingUser == null)
            return NotFound();

        if (user.CurriculumId != -1)
        {
            var newCurriculum = SdmCurriculum.GetBy(user.CurriculumId);
            if (newCurriculum == null)
                return NotFound();
            existingUser.Curriculum = newCurriculum;
        }

        SdmUser.UpdateBy(existingUser);

        return Ok(user);
    }

    public class DtoUpdateUserCurriculum
    {
        public required int Id { get; init; } = -1;
        public required int CurriculumId { get; init; } = -1;
    }
    #endregion
}
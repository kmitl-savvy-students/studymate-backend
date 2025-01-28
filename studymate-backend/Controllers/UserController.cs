using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    #region [POST] Update User
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPut("update")]
    public ActionResult<User> Update([FromBody] DtoUpdateUser user)
    {
        var existingUser = SdmUser.GetBy(user.Id);
        if (existingUser == null)
            return NotFound();

        if (user.CurriculumId != null)
        {
            var newCurriculum = SdmCurriculum.GetBy(user.CurriculumId ?? -1);
            if (newCurriculum == null)
                return NotFound();
            existingUser.Curriculum = newCurriculum;
        }

        existingUser.NameNick = user.NameNick ?? existingUser.NameNick;
        existingUser.NameFirst = user.NameFirst ?? existingUser.NameFirst;
        existingUser.NameLast = user.NameLast ?? existingUser.NameLast;
        existingUser.Profile = user.Profile ?? existingUser.Profile;

        SdmUser.UpdateBy(existingUser);

        return Ok(user);
    }

    public class DtoUpdateUser(int id, string? nameNick, string? nameFirst, string? nameLast, string? profile, int? curriculumId)
    {
        public required int Id { get; init; } = id;
        public string? NameNick { get; } = nameNick;
        public string? NameFirst { get; } = nameFirst;
        public string? NameLast { get; } = nameLast;
        public string? Profile { get; } = profile;
        public int? CurriculumId { get; } = curriculumId;
    }
    #endregion
}
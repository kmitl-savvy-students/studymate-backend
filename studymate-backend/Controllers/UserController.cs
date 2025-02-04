using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPatch("update")]
    public ActionResult<User> Update([FromBody] DtoUpdateUser user)
    {
        var existingUser = SdmUser.GetBy(user.id);
        if (existingUser == null)
            return NotFound(new { message = "User not found" });

        if (user.curriculumId != null)
        {
            var newCurriculum = SdmCurriculum.GetBy(user.curriculumId ?? -1);
            if (newCurriculum == null)
                return NotFound(new { message = "Curriculum not found" });
            existingUser.Curriculum = newCurriculum;
        }

        existingUser.NameNick = user.nameNick ?? existingUser.NameNick;
        existingUser.NameFirst = user.nameFirst ?? existingUser.NameFirst;
        existingUser.NameLast = user.nameLast ?? existingUser.NameLast;
        existingUser.Profile = user.profile ?? existingUser.Profile;

        SdmUser.Update(existingUser);

        return Ok(user);
    }

    public class DtoUpdateUser
    {
        public required string id { get; set; }
        public string? nameNick { get; set; }
        public string? nameFirst { get; set; }
        public string? nameLast { get; set; }
        public string? profile { get; set; }
        public int? curriculumId { get; set; }
    }
}

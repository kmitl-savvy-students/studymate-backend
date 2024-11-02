using System.Security.Claims;
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
        var userAuthorized = SdmUser.GetById(ClaimTypes.NameIdentifier);
        if (userAuthorized == null)
            return Unauthorized();

        var existingUser = SdmUser.GetById(user.id);
        if (existingUser == null)
            return NotFound("User not found.");

        if (user.curriculumId != null)
        {
            var newCurriculum = SdmCurriculum.GetById(user.curriculumId ?? -1);
            if (newCurriculum == null)
                return NotFound("Curriculum not found.");
            existingUser.curriculum = newCurriculum;
        }

        existingUser.nameNick = user.nameFirst ?? existingUser.nameNick;
        existingUser.nameFirst = user.nameFirst ?? existingUser.nameFirst;
        existingUser.nameLast = user.nameFirst ?? existingUser.nameLast;
        existingUser.profile = user.nameFirst ?? existingUser.profile;

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

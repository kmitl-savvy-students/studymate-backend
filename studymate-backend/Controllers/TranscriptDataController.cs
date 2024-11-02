using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/transcript")]
public class TranscriptDataController : ControllerBase
{
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPost("get-by-user")]
    public ActionResult<IEnumerable<TranscriptData>> Get(DtoUser dtoUser)
    {
        // Verify user
        var user = SdmUser.GetBy(dtoUser.userId);
        if (user == null)
            return Unauthorized(new { message = "User not found." });

        var transcriptDatas = SdmTranscriptData.GetBy(user);

        return Ok(transcriptDatas);
    }

    public class DtoUser
    {
        public required string userId { get; set; }
    }
}

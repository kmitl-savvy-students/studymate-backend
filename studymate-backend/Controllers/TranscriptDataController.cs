using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Helper;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/transcript")]
public class TranscriptDataController : ControllerBase
{
    [HttpPost("get-by-user")]
    public ActionResult<IEnumerable<TranscriptData>> Get(DtoUser dtoUser)
    {
        var userTokenId = SdmString.CleanAndTrim(dtoUser.userTokenId);

        if (!SdmString.IsValid(userTokenId, 64, 64))
            return BadRequest(new { message = "Invalid user token." });

        // Verify token
        var userToken = SdmUserToken.GetBy(userTokenId);
        if (userToken == null)
            return Unauthorized(new { message = "User not found." });

        var transcriptDatas = SdmTranscriptData.GetBy(userToken.user);

        return Ok(transcriptDatas);
    }

    public class DtoUser
    {
        public required string userTokenId { get; set; }
    }
}

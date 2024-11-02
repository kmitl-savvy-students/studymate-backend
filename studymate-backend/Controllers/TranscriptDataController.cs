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

    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpDelete("delete/{userId}")]
    public ActionResult Delete(string userId)
    {
        // Verify user
        var user = SdmUser.GetBy(userId);
        if (user == null)
            return Unauthorized(new { message = "User not found." });

        // Get All Transcripts
        var transcripts = SdmTranscript.GetAllBy(user);
        foreach (var transcript in transcripts)
            SdmTranscriptData.DeleteByTranscript(transcript);
        SdmTranscript.DeleteByUser(user);

        return Ok(new { message = "Transcript data deleted." });
    }

    public class DtoUser
    {
        public required string userId { get; set; }
    }
}

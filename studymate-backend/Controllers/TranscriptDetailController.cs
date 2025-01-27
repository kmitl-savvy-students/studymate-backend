using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/transcript")]
public class TranscriptDetailController : ControllerBase
{
    #region [GET] Transcript Details
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpGet("get/{userId:int}")]
    public ActionResult<IEnumerable<TranscriptDetail>> Get(int userId)
    {
        var user = SdmUser.GetBy(userId);
        if (user == null)
            return Unauthorized();

        var transcript = SdmTranscript.GetBy(user);
        var transcriptDatas = SdmTranscriptDetail.GetAllBy(transcript);

        return Ok(transcriptDatas);
    }
    #endregion
    #region [DELETE] Transcript Details
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpDelete("delete/{userId:int}")]
    public ActionResult Delete(int userId)
    {
        var user = SdmUser.GetBy(userId);
        if (user == null)
            return Unauthorized();

        var transcripts = SdmTranscript.GetAllBy(user);
        foreach (var transcript in transcripts)
            SdmTranscriptDetail.DeleteBy(transcript);
        SdmTranscript.DeleteBy(user);

        return Ok();
    }
    #endregion
}
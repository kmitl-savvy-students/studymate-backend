using Microsoft.AspNetCore.Mvc;
using studymate_backend.Controllers.Core;
using studymate_backend.Enums;
using studymate_backend.Helper;
using studymate_backend.Models.Core;
using studymate_backend.Models.StudyMate.Raw.Request;
using studymate_backend.Services;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/transcript")]
public class TranscriptDataController(
    TranscriptDataService transcriptDataService,
    UserTokenService userTokenService
) : IController
{
    [HttpPost("get-by-user")]
    public BaseResponse Get(RequestUser requestUser)
    {
        if (!SdmString.IsValid(requestUser.UserTokenId, 64, 64))
            return new BaseResponse(EnumResponseCode.FIELDS_INVALID);

        var userTokenId = SdmString.cleanAndTrim(requestUser.UserTokenId);

        // Verify token
        var userToken = userTokenService.Get(userTokenId);
        if (userToken == null)
            return new BaseResponse(EnumResponseCode.UNAUTHORIZED);

        var transcriptDatas = transcriptDataService.GetByUser(userToken.User);
        
        return new BaseResponse(EnumResponseCode.OK, transcriptDatas);
    }
}
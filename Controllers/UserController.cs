using Microsoft.AspNetCore.Mvc;
using studymate_backend.Controllers.Core;
using studymate_backend.Enums;
using studymate_backend.Helper;
using studymate_backend.Models.Core;
using studymate_backend.Models.StudyMate.Raw.Request;
using studymate_backend.Services;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/user")]
public class UserController(UserTokenService userTokenService) : BaseController
{
    [HttpPost]
    public BaseResponse Get(RequestUser requestUser)
    {
        if (!SDMString.IsValid(requestUser.UserTokenId, 64, 64))
            return new BaseResponse(EnumResponseCode.FIELDS_INVALID);

        var userTokenId = SDMString.cleanAndTrim(requestUser.UserTokenId);

        // Verify token
        var userToken = userTokenService.Get(userTokenId);
        if (userToken == null)
            return new BaseResponse(EnumResponseCode.UNAUTHORIZED);

        return new BaseResponse(EnumResponseCode.OK, userToken.User.Serialized());
    }

    [HttpGet]
    public Task<string> Test()
    {
        return new SDMVertexAI().GenerateContent();
    }
}
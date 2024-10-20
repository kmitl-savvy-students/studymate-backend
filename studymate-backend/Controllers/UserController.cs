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
public class UserController(UserTokenService userTokenService) : IController
{
    [HttpPost]
    public BaseResponse Get(RequestUser requestUser)
    {
        if (!SdmString.IsValid(requestUser.UserTokenId, 64, 64))
            return new BaseResponse(EnumResponseCode.FIELDS_INVALID);

        var userTokenId = SdmString.cleanAndTrim(requestUser.UserTokenId);

        // Verify token
        var userToken = userTokenService.Get(userTokenId);
        if (userToken == null)
            return new BaseResponse(EnumResponseCode.UNAUTHORIZED);
        
        // Remove password
        userToken.User.Password = "";
        
        return new BaseResponse(EnumResponseCode.OK, userToken.User.Serialized());
    }
}
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Controllers.Core;
using studymate_backend.Enums;
using studymate_backend.Helper;
using studymate_backend.Models.Core;
using studymate_backend.Models.StudyMate.Object;
using studymate_backend.Models.StudyMate.Raw.Request.Auth;
using studymate_backend.Services;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/google")]
public class GoogleOAuthController(
    UserService userService,
    UserTokenService userTokenService
) : BaseController
{
    [HttpPost]
    public async Task<BaseResponse> Continue(RequestContinueWithGoogle request)
    {
        var googleToken = SDMString.cleanAndTrim(request.GoogleToken);

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(googleToken);

            var id = payload.Email.Split('@')[0];
            var domain = payload.HostedDomain;

            // Verify KMITL user
            if (domain != "kmitl.ac.th" || !SDMNumber.IsValid(id) || !SDMString.IsValid(id, 8, 8))
                return new BaseResponse(EnumResponseCode.UNAUTHORIZED);

            var user = userService.Get(id);
            if (user == null)
            {
                // Create user if user doesn't exist
                user = new User(
                    id,
                    SDMAuthentication.passwordHash(payload.Subject),
                    EnumGender.OTHER,
                    payload.GivenName,
                    payload.GivenName,
                    payload.FamilyName
                );
                userService.Add(user);
            }

            // Generate token string
            var randomizeToken = SDMString.generateRandomToken();
            while (userTokenService.Get(randomizeToken) != null)
                randomizeToken = SDMString.generateRandomToken();

            // Verify if token is already exists
            var userToken = userTokenService.GetByUser(user);
            if (userToken != null)
                userTokenService.Remove(userToken);

            // Create token
            userToken = new UserToken(
                randomizeToken,
                user,
                SDMDateTime.Now(),
                SDMDateTime.Now().AddHours(12)
            );
            userTokenService.Add(userToken);

            return new BaseResponse(EnumResponseCode.CREATED, userToken.Serialized());
        }
        catch (InvalidJwtException)
        {
            return new BaseResponse(EnumResponseCode.UNAUTHORIZED);
        }
    }
}
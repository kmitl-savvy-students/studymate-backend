using Microsoft.AspNetCore.Mvc;
using studymate_backend.Controllers.Core;
using studymate_backend.Enums;
using studymate_backend.Enums.Core;
using studymate_backend.Helper;
using studymate_backend.Models.Core;
using studymate_backend.Models.StudyMate.Object;
using studymate_backend.Models.StudyMate.Raw.Request.Auth;
using studymate_backend.Services;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserService userService,
    UserTokenService userTokenService
) : IController
{
    [HttpPost("sign-up")]
    public BaseResponse SignUp(RequestSignUp requestSignUp)
    {
        if (!SdmNumber.IsValid(requestSignUp.Id) ||
            !SdmString.IsValid(requestSignUp.Id, 8, 8) ||
            !SdmString.IsValid(requestSignUp.Password, 64) ||
            !SdmString.IsValid(requestSignUp.PasswordConfirm, 64) ||
            !SdmString.IsValid(requestSignUp.NameNick, 256) ||
            !SdmString.IsValid(requestSignUp.NameFirst, 256) ||
            !SdmString.IsValid(requestSignUp.NameLast, 256) ||
            !SdmString.IsValid(requestSignUp.Gender, 6) ||
            !SdmString.IsValid(requestSignUp.Profile, 256))
            return new BaseResponse(EnumResponseCode.FIELDS_INVALID);

        var id = SdmString.cleanAndTrim(requestSignUp.Id);
        var password = SdmString.cleanAndTrim(requestSignUp.Password);
        var passwordConfirm = SdmString.cleanAndTrim(requestSignUp.PasswordConfirm);
        var gender = BaseEnum.Get<EnumGender>(SdmString.cleanAndTrim(requestSignUp.Gender)) ?? EnumGender.OTHER;
        var nameNick = SdmString.cleanAndTrim(requestSignUp.NameNick);
        var nameFirst = SdmString.cleanAndTrim(requestSignUp.NameFirst);
        var nameLast = SdmString.cleanAndTrim(requestSignUp.NameLast);
        var profile = SdmString.cleanAndTrim(requestSignUp.Profile);

        // Check if id is already exists
        if (userService.Get(id) != null)
            return new BaseResponse(EnumResponseCode.DUPLICATE_ID);

        // Verify password
        if (password != passwordConfirm)
            return new BaseResponse(EnumResponseCode.PASSWORD_MISMATCH);
        if (!SdmAuthentication.isPasswordStrong(password))
            return new BaseResponse(EnumResponseCode.PASSWORD_WEAK);

        // Create user
        userService.Add(new User(
            id,
            SdmAuthentication.passwordHash(password),
            gender,
            nameNick,
            nameFirst,
            nameLast,
            profile
        ));

        return new BaseResponse(EnumResponseCode.CREATED);
    }

    [HttpPost("sign-out")]
    public BaseResponse SignOut(RequestSignOut requestSignOut)
    {
        if (!SdmString.IsValid(requestSignOut.UserTokenId, 64, 64))
            return new BaseResponse(EnumResponseCode.OK);

        var userTokenId = SdmString.cleanAndTrim(requestSignOut.UserTokenId);

        // Find token to remove
        var userToken = userTokenService.Get(userTokenId);
        if (userToken == null)
            return new BaseResponse(EnumResponseCode.OK);

        userTokenService.Remove(userToken);
        return new BaseResponse(EnumResponseCode.OK);
    }

    [HttpPost("sign-in")]
    public BaseResponse SignIn(RequestSignIn requestSignIn)
    {
        if (!SdmNumber.IsValid(requestSignIn.Id) ||
            !SdmString.IsValid(requestSignIn.Id, 8, 8) ||
            !SdmString.IsValid(requestSignIn.Password, 64))
            return new BaseResponse(EnumResponseCode.FIELDS_INVALID);

        var id = SdmString.cleanAndTrim(requestSignIn.Id);
        var password = SdmString.cleanAndTrim(requestSignIn.Password);

        // Find user to authenticate
        var user = userService.Get(id);
        if (user == null)
            return new BaseResponse(EnumResponseCode.NOT_FOUND);

        // Verify password
        if (!SdmAuthentication.passwordVerify(password, user.Password))
            return new BaseResponse(EnumResponseCode.NOT_FOUND);

        // Generate token string
        var randomizeToken = SdmString.generateRandomToken();
        while (userTokenService.Get(randomizeToken) != null)
            randomizeToken = SdmString.generateRandomToken();

        // Verify if token is already exists
        var userToken = userTokenService.GetByUser(user);
        if (userToken != null)
            userTokenService.Remove(userToken);

        // Create new token
        userToken = new UserToken(
            randomizeToken,
            user,
            SdmDateTime.Now(),
            SdmDateTime.Now().AddHours(12)
        );
        userTokenService.Add(userToken);

        return new BaseResponse(EnumResponseCode.CREATED, userToken.Serialized());
    }
    
    [HttpPost("token")]
    public BaseResponse Token(RequestToken requestToken)
    {
        if (!SdmString.IsValid(requestToken.UserTokenId, 64, 64))
            return new BaseResponse(EnumResponseCode.FIELDS_INVALID);

        var userTokenId = SdmString.cleanAndTrim(requestToken.UserTokenId);

        // Verify token
        var userToken = userTokenService.Get(userTokenId);
        if (userToken == null)
            return new BaseResponse(EnumResponseCode.NOT_FOUND);
        
        return new BaseResponse(EnumResponseCode.OK, userToken.Serialized());
    }
}
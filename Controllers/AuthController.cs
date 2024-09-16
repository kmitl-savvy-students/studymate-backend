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
) : BaseController
{
    [HttpPost("sign-up")]
    public BaseResponse SignUp(RequestSignUp requestSignUp)
    {
        if (!SDMNumber.IsValid(requestSignUp.Id) ||
            !SDMString.IsValid(requestSignUp.Id, 8, 8) ||
            !SDMString.IsValid(requestSignUp.Password, 64) ||
            !SDMString.IsValid(requestSignUp.PasswordConfirm, 64) ||
            !SDMString.IsValid(requestSignUp.NameNick, 256) ||
            !SDMString.IsValid(requestSignUp.NameFirst, 256) ||
            !SDMString.IsValid(requestSignUp.NameLast, 256) ||
            !SDMString.IsValid(requestSignUp.Gender, 6))
            return new BaseResponse(EnumResponseCode.FIELDS_INVALID);

        var id = SDMString.cleanAndTrim(requestSignUp.Id);
        var password = SDMString.cleanAndTrim(requestSignUp.Password);
        var passwordConfirm = SDMString.cleanAndTrim(requestSignUp.PasswordConfirm);
        var gender = BaseEnum.Get<EnumGender>(SDMString.cleanAndTrim(requestSignUp.Gender)) ?? EnumGender.OTHER;
        var nameNick = SDMString.cleanAndTrim(requestSignUp.NameNick);
        var nameFirst = SDMString.cleanAndTrim(requestSignUp.NameFirst);
        var nameLast = SDMString.cleanAndTrim(requestSignUp.NameLast);

        // Check if id is already exists
        if (userService.Get(id) != null)
            return new BaseResponse(EnumResponseCode.DUPLICATE_ID);

        // Verify password
        if (password != passwordConfirm)
            return new BaseResponse(EnumResponseCode.PASSWORD_MISMATCH);
        if (!SDMAuthentication.isPasswordStrong(password))
            return new BaseResponse(EnumResponseCode.PASSWORD_WEAK);

        // Create user
        userService.Add(new User(
            id,
            SDMAuthentication.passwordHash(password),
            gender,
            nameNick,
            nameFirst,
            nameLast
        ));

        return new BaseResponse(EnumResponseCode.CREATED);
    }

    [HttpPost("sign-out")]
    public BaseResponse SignOut(RequestSignOut requestSignOut)
    {
        if (!SDMString.IsValid(requestSignOut.UserTokenId, 64, 64))
            return new BaseResponse(EnumResponseCode.FIELDS_INVALID);

        var userTokenId = SDMString.cleanAndTrim(requestSignOut.UserTokenId);

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
        if (!SDMNumber.IsValid(requestSignIn.Id) ||
            !SDMString.IsValid(requestSignIn.Id, 8, 8) ||
            !SDMString.IsValid(requestSignIn.Password, 64))
            return new BaseResponse(EnumResponseCode.FIELDS_INVALID);

        var id = SDMString.cleanAndTrim(requestSignIn.Id);
        var password = SDMString.cleanAndTrim(requestSignIn.Password);

        // Find user to authenticate
        var user = userService.Get(id);
        if (user == null)
            return new BaseResponse(EnumResponseCode.NOT_FOUND);

        // Verify password
        if (!SDMAuthentication.passwordVerify(password, user.Password))
            return new BaseResponse(EnumResponseCode.NOT_FOUND);

        // Generate token string
        var randomizeToken = SDMString.generateRandomToken();
        while (userTokenService.Get(randomizeToken) != null)
            randomizeToken = SDMString.generateRandomToken();

        // Verify if token is already exists
        var userToken = userTokenService.GetByUser(user);
        if (userToken != null)
            userTokenService.Remove(userToken);

        // Create new token
        userToken = new UserToken(
            randomizeToken,
            user,
            SDMDateTime.Now(),
            SDMDateTime.Now().AddHours(12)
        );
        userTokenService.Add(userToken);

        return new BaseResponse(EnumResponseCode.CREATED, userToken.Serialized());
    }
}
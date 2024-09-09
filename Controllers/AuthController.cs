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
[Route("api/[controller]")]
public class AuthController(AuthService authService, UserService userService) : BaseController
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

        if (userService.Get(id) != null)
            return new BaseResponse(EnumResponseCode.DUPLICATE_ID);
        if (password != passwordConfirm)
            return new BaseResponse(EnumResponseCode.PASSWORD_MISMATCH);

        if (!SDMAuthentication.isPasswordStrong(password))
            return new BaseResponse(EnumResponseCode.PASSWORD_WEAK);

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

    [HttpPost("sign-in")]
    public BaseResponse SignIn(RequestSignIn requestSignIn)
    {
        return new BaseResponse(EnumResponseCode.CREATED);
    }
}
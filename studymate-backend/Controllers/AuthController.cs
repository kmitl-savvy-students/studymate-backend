using Microsoft.AspNetCore.Mvc;
using studymate_backend.Helper;
using studymate_backend.Libraries.Enums;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    [HttpPost("sign-up")]
    public IActionResult SignUp([FromBody] SignUpDto signUpDto)
    {
        if (!SdmNumber.IsValid(signUpDto.id) ||
            !SdmString.IsValid(signUpDto.id, 8, 8) ||
            !SdmString.IsValid(signUpDto.password, 64) ||
            !SdmString.IsValid(signUpDto.passwordConfirm, 64) ||
            !SdmString.IsValid(signUpDto.gender, 6) ||
            !SdmString.IsValid(signUpDto.nameNick, 256) ||
            !SdmString.IsValid(signUpDto.nameFirst, 256) ||
            !SdmString.IsValid(signUpDto.nameLast, 256))
            return BadRequest("Invalid request data.");

        var id = SdmString.cleanAndTrim(signUpDto.id);
        var password = SdmString.cleanAndTrim(signUpDto.password);
        var passwordConfirm = SdmString.cleanAndTrim(signUpDto.passwordConfirm);
        var gender = EnumBase.Get<EnumGender>(SdmString.cleanAndTrim(signUpDto.gender)) ?? EnumGender.OTHER;
        var nameNick = SdmString.cleanAndTrim(signUpDto.nameNick);
        var nameFirst = SdmString.cleanAndTrim(signUpDto.nameFirst);
        var nameLast = SdmString.cleanAndTrim(signUpDto.nameLast);

        if (SdmUser.getById(id) != null)
            return Conflict("User with the given ID already exists.");

        if (password != passwordConfirm)
            return BadRequest("Password mismatch.");
        if (!SdmAuthentication.isPasswordStrong(password))
            return BadRequest("Password does not meet strength requirements.");

        var user = new User(
            id,
            SdmAuthentication.passwordHash(password),
            gender,
            nameNick,
            nameFirst,
            nameLast,
            "",
            null
        );

        SdmUser.insert(user);
        return Ok("User created.");
    }

    // [HttpPost("sign-out")]
    // public BaseResponse SignOut(RequestSignOut requestSignOut)
    // {
    //     if (!SdmString.IsValid(requestSignOut.UserTokenId, 64, 64))
    //         return new BaseResponse(EnumResponseCode.FIELDS_INVALID);
    //
    //     var userTokenId = SdmString.cleanAndTrim(requestSignOut.UserTokenId);
    //
    //     // Find token to remove
    //     var userToken = userTokenService.Get(userTokenId);
    //     if (userToken == null)
    //         return new BaseResponse(EnumResponseCode.NOT_FOUND);
    //
    //     userTokenService.Remove(userToken);
    //     return new BaseResponse(EnumResponseCode.OK);
    // }
    //
    // [HttpPost("sign-in")]
    // public BaseResponse SignIn(RequestSignIn requestSignIn)
    // {
    //     if (!SdmNumber.IsValid(requestSignIn.Id) ||
    //         !SdmString.IsValid(requestSignIn.Id, 8, 8) ||
    //         !SdmString.IsValid(requestSignIn.Password, 64))
    //         return new BaseResponse(EnumResponseCode.FIELDS_INVALID);
    //
    //     var id = SdmString.cleanAndTrim(requestSignIn.Id);
    //     var password = SdmString.cleanAndTrim(requestSignIn.Password);
    //
    //     // Find user to authenticate
    //     var user = userService.Get(id);
    //     if (user == null)
    //         return new BaseResponse(EnumResponseCode.NOT_FOUND);
    //
    //     // Verify password
    //     if (!SdmAuthentication.passwordVerify(password, user.Password))
    //         return new BaseResponse(EnumResponseCode.NOT_FOUND);
    //
    //     // Generate token string
    //     var randomizeToken = SdmString.generateRandomToken();
    //     while (userTokenService.Get(randomizeToken) != null)
    //         randomizeToken = SdmString.generateRandomToken();
    //
    //     // Verify if token is already exists
    //     var userToken = userTokenService.GetByUser(user);
    //     if (userToken != null)
    //         userTokenService.Remove(userToken);
    //
    //     // Create new token
    //     userToken = new UserToken(
    //         randomizeToken,
    //         user,
    //         SdmDateTime.Now(),
    //         SdmDateTime.Now().AddHours(12)
    //     );
    //     userTokenService.Add(userToken);
    //
    //     return new BaseResponse(EnumResponseCode.CREATED, userToken.Serialized());
    // }
    //
    // [HttpPost("token")]
    // public BaseResponse Token(RequestToken requestToken)
    // {
    //     if (!SdmString.IsValid(requestToken.UserTokenId, 64, 64))
    //         return new BaseResponse(EnumResponseCode.FIELDS_INVALID);
    //
    //     var userTokenId = SdmString.cleanAndTrim(requestToken.UserTokenId);
    //
    //     // Verify token
    //     var userToken = userTokenService.Get(userTokenId);
    //     if (userToken == null)
    //         return new BaseResponse(EnumResponseCode.NOT_FOUND);
    //
    //     return new BaseResponse(EnumResponseCode.OK, userToken.Serialized());
    // }

    public class SignUpDto
    {
        public required string id { get; set; }
        public required string password { get; set; }
        public required string passwordConfirm { get; set; }
        public required string gender { get; set; }
        public required string nameNick { get; set; }
        public required string nameFirst { get; set; }
        public required string nameLast { get; set; }
    }
}

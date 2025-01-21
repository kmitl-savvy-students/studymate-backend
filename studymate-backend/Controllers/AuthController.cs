using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Helper;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;
using SdmAuthentication = studymate_backend.Libraries.Helper.SdmAuthentication;
using SdmString = studymate_backend.Libraries.Helper.SdmString;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    #region Sign Up
    [AllowAnonymous]
    [HttpPost("sign-up")]
    public ActionResult SignUp([FromBody] DtoSignUp dtoSignUp)
    {
        var id = SdmString.CleanAndTrim(dtoSignUp.Id);
        var password = SdmString.CleanAndTrim(dtoSignUp.Password);
        var passwordConfirm = SdmString.CleanAndTrim(dtoSignUp.PasswordConfirm);
        var nameNick = SdmString.CleanAndTrim(dtoSignUp.NameNick);
        var nameFirst = SdmString.CleanAndTrim(dtoSignUp.NameFirst);
        var nameLast = SdmString.CleanAndTrim(dtoSignUp.NameLast);

        if (!SdmNumber.IsValid(id) ||
            !SdmString.IsValid(id, 8, 8) ||
            !SdmString.IsValid(password, 64) ||
            !SdmString.IsValid(passwordConfirm, 64) ||
            !SdmString.IsValid(nameNick, 256) ||
            !SdmString.IsValid(nameFirst, 256) ||
            !SdmString.IsValid(nameLast, 256))
            return BadRequest(new { message = "Invalid request data." });

        if (SdmUser.GetBy(id) != null)
            return Conflict(new { message = "User with the given ID already exists." });

        if (password != passwordConfirm)
            return BadRequest("Password mismatch.");
        if (!SdmAuthentication.IsPasswordStrong(password))
            return BadRequest(new { message = "Password does not meet strength requirements." });

        SdmUser.Insert(new User(
            id,
            SdmAuthentication.PasswordHash(password),
            nameNick,
            nameFirst,
            nameLast,
            "",
            null
        ));
        return Ok(new { message = "User created." });
    }
    public class DtoSignUp
    {
        public required string Id { get; init; } = string.Empty;
        public required string Password { get; init; } = string.Empty;
        public required string PasswordConfirm { get; init; } = string.Empty;
        public required string NameNick { get; init; } = string.Empty;
        public required string NameFirst { get; init; } = string.Empty;
        public required string NameLast { get; init; } = string.Empty;
    }
    #endregion
    #region Sign In
    [AllowAnonymous]
    [HttpPost("sign-in")]
    public ActionResult<UserToken> SignIn(DtoSignIn dtoSignIn)
    {
        var id = SdmString.CleanAndTrim(dtoSignIn.Id);
        var password = SdmString.CleanAndTrim(dtoSignIn.Password);

        if (!SdmNumber.IsValid(id) ||
            !SdmString.IsValid(id, 8, 8) ||
            !SdmString.IsValid(password, 64))
            return BadRequest(new { message = "Invalid request data." });

        var user = SdmUser.GetBy(id);
        if (user == null)
            return NotFound(new { message = "Incorrect username or password." });

        if (!SdmAuthentication.PasswordVerify(password, user.Password))
            return NotFound(new { message = "Incorrect username or password." });

        var randomizeToken = SdmString.GenerateRandomToken();
        while (SdmUserToken.GetBy(randomizeToken) != null)
            randomizeToken = SdmString.GenerateRandomToken();

        var userToken = SdmUserToken.GetBy(user);
        if (userToken != null)
            SdmUserToken.Delete(userToken);

        userToken = new UserToken(
            randomizeToken,
            user,
            SdmDateTime.Now(),
            SdmDateTime.Now().AddHours(12)
        );
        SdmUserToken.Insert(userToken);

        return Ok(userToken);
    }
    public class DtoSignIn
    {
        public required string Id { get; init; } = string.Empty;
        public required string Password { get; init; } = string.Empty;
    }
    #endregion
    #region Sign Out
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPost("sign-out")]
    public ActionResult SignOut(DtoSignOut dtoSignOut)
    {
        var userTokenId = SdmString.CleanAndTrim(dtoSignOut.UserTokenId);

        if (!SdmString.IsValid(userTokenId, 64, 64))
            return BadRequest(new { message = "Invalid request data." });

        var userToken = SdmUserToken.GetBy(userTokenId);
        if (userToken == null)
            return NotFound(new { message = "Incorrect user token." });

        SdmUserToken.Delete(userToken);
        return Ok(new { message = "Sign out successfully." });
    }
    public class DtoSignOut
    {
        public required string UserTokenId { get; init; } = string.Empty;
    }
    #endregion

    #region Token Refresh
    [AllowAnonymous]
    [HttpPost("token")]
    public ActionResult<UserToken> Token(DtoToken dtoToken)
    {
        var userTokenId = SdmString.CleanAndTrim(dtoToken.UserTokenId);

        if (!SdmString.IsValid(userTokenId, 64, 64))
            return BadRequest(new { message = "Invalid request data." });

        var userToken = SdmUserToken.GetBy(userTokenId);
        if (userToken == null)
            return NotFound(new { message = "Incorrect user token." });

        return Ok(userToken);
    }
    public class DtoToken
    {
        public required string UserTokenId { get; init; } = string.Empty;
    }
    #endregion
}

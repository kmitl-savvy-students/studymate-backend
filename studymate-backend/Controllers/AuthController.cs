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
    [AllowAnonymous]
    [HttpPost("sign-up")]
    public ActionResult SignUp([FromBody] DtoSignUp dtoSignUp)
    {
        var id = SdmString.CleanAndTrim(dtoSignUp.id);
        var password = SdmString.CleanAndTrim(dtoSignUp.password);
        var passwordConfirm = SdmString.CleanAndTrim(dtoSignUp.passwordConfirm);
        var nameNick = SdmString.CleanAndTrim(dtoSignUp.nameNick);
        var nameFirst = SdmString.CleanAndTrim(dtoSignUp.nameFirst);
        var nameLast = SdmString.CleanAndTrim(dtoSignUp.nameLast);

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

    [AllowAnonymous]
    [HttpPost("sign-in")]
    public ActionResult<UserToken> SignIn(DtoSignIn dtoSignIn)
    {
        var id = SdmString.CleanAndTrim(dtoSignIn.id);
        var password = SdmString.CleanAndTrim(dtoSignIn.password);

        if (!SdmNumber.IsValid(id) ||
            !SdmString.IsValid(id, 8, 8) ||
            !SdmString.IsValid(password, 64))
            return BadRequest("Invalid request data.");

        // Find user to authenticate
        var user = SdmUser.GetBy(id);
        if (user == null)
            return NotFound("Incorrect username or password.");

        // Verify password
        if (!SdmAuthentication.PasswordVerify(password, user.password))
            return NotFound("Incorrect username or password.");

        // Generate token string
        var randomizeToken = SdmString.GenerateRandomToken();
        while (SdmUserToken.GetBy(randomizeToken) != null)
            randomizeToken = SdmString.GenerateRandomToken();

        // Verify if token is already exists
        var userToken = SdmUserToken.GetBy(user);
        if (userToken != null)
            SdmUserToken.Delete(userToken);

        // Create token
        userToken = new UserToken(
            randomizeToken,
            user,
            SdmDateTime.Now(),
            SdmDateTime.Now().AddHours(12)
        );
        SdmUserToken.Insert(userToken);

        return Ok(userToken);
    }

    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPost("sign-out")]
    public ActionResult SignOut(DtoSignOut dtoSignOut)
    {
        var userTokenId = SdmString.CleanAndTrim(dtoSignOut.userTokenId);

        if (!SdmString.IsValid(userTokenId, 64, 64))
            return BadRequest("Invalid request data.");

        // Find token to remove
        var userToken = SdmUserToken.GetBy(userTokenId);
        if (userToken == null)
            return NotFound("Incorrect user token.");

        SdmUserToken.Delete(userToken);
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("token")]
    public ActionResult<UserToken> Token(DtoToken dtoToken)
    {
        var userTokenId = SdmString.CleanAndTrim(dtoToken.userTokenId);

        if (!SdmString.IsValid(userTokenId, 64, 64))
            return BadRequest("Invalid request data.");

        // Find token to remove
        var userToken = SdmUserToken.GetBy(userTokenId);
        if (userToken == null)
            return NotFound("Incorrect user token.");

        return Ok(userToken);
    }

    public class DtoSignUp
    {
        public required string id { get; set; }
        public required string password { get; set; }
        public required string passwordConfirm { get; set; }
        public required string nameNick { get; set; }
        public required string nameFirst { get; set; }
        public required string nameLast { get; set; }
    }

    public class DtoSignIn
    {
        public required string id { get; set; }
        public required string password { get; set; }
    }

    public class DtoSignOut
    {
        public required string userTokenId { get; set; }
    }

    public class DtoToken
    {
        public required string userTokenId { get; set; }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Helper;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    #region [POST] Sign Up
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
            return BadRequest(new { message = "ข้อมูลไม่ถูกต้อง" });

        var idInt = int.Parse(id);

        if (SdmUser.GetBy(idInt) != null)
            return Conflict(new { message = "รหัสผู้ใช้งานถูกสมัครสมาชิกไว้อยู่แล้ว" });

        if (password != passwordConfirm)
            return BadRequest("ยืนยันรหัสผ่านไม่ถูกต้อง");
        if (!SdmAuthentication.IsPasswordStrong(password))
            return BadRequest(new { message = "รหัสผ่านไม่แข็งแรงพอ" });

        SdmUser.Insert(new User(
            idInt,
            SdmAuthentication.PasswordHash(password),
            nameNick,
            nameFirst,
            nameLast,
            "",
            false,
            null
        ));
        return Ok();
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
    #region [POST] Sign In
    [AllowAnonymous]
    [HttpPost("sign-in")]
    public ActionResult<UserToken> SignIn(DtoSignIn dtoSignIn)
    {
        var id = SdmString.CleanAndTrim(dtoSignIn.Id);
        var password = SdmString.CleanAndTrim(dtoSignIn.Password);

        if (!SdmNumber.IsValid(id) ||
            !SdmString.IsValid(id, 8, 8) ||
            !SdmString.IsValid(password, 64))
            return BadRequest(new { message = "ข้อมูลไม่ถูกต้อง" });

        var idInt = int.Parse(id);

        var user = SdmUser.GetBy(idInt);
        if (user == null)
            return BadRequest(new { message = "ชื่อผู้ใช้หรือรหัสผ่านผิดพลาด" });

        if (!SdmAuthentication.PasswordVerify(password, user.Password))
            return BadRequest(new { message = "ชื่อผู้ใช้หรือรหัสผ่านผิดพลาด" });

        var randomizeToken = SdmString.GenerateRandomToken();
        while (SdmUserToken.GetBy(randomizeToken) != null)
            randomizeToken = SdmString.GenerateRandomToken();

        var userToken = SdmUserToken.GetBy(user);
        if (userToken != null)
            SdmUserToken.DeleteBy(userToken);

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
    #region [POST] Sign Out
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPost("sign-out")]
    public ActionResult SignOut(DtoSignOut dtoSignOut)
    {
        var userTokenId = SdmString.CleanAndTrim(dtoSignOut.UserTokenId);

        if (!SdmString.IsValid(userTokenId, 64, 64))
            return BadRequest(new { message = "ข้อมูลไม่ถูกต้อง" });

        var userToken = SdmUserToken.GetBy(userTokenId);
        if (userToken == null)
            return BadRequest(new { message = "ไม่พบเซสชัน" });

        SdmUserToken.DeleteBy(userToken);
        return Ok();
    }

    public class DtoSignOut
    {
        public required string UserTokenId { get; init; } = string.Empty;
    }
    #endregion

    #region [POST] Token Refresh
    [AllowAnonymous]
    [HttpPost("token")]
    public ActionResult<UserToken> Token(DtoToken dtoToken)
    {
        var userTokenId = SdmString.CleanAndTrim(dtoToken.UserTokenId);

        if (!SdmString.IsValid(userTokenId, 64, 64))
            return BadRequest(new { message = "ข้อมูลไม่ถูกต้อง" });

        var userToken = SdmUserToken.GetBy(userTokenId);
        if (userToken == null)
            return BadRequest(new { message = "ข้อมูลไม่ถูกต้อง" });

        return Ok(userToken);
    }
    
    [HttpPost("sign-up-test")]
    public ActionResult SignUpTest([FromBody] DtoSignUpTest dtoSignUp)
    {
        var id = SdmString.CleanAndTrim(dtoSignUp.Id);
        var password = SdmString.CleanAndTrim(dtoSignUp.Password);
        var passwordConfirm = SdmString.CleanAndTrim(dtoSignUp.PasswordConfirm);
        var nameNick = SdmString.CleanAndTrim(dtoSignUp.NameNick);
        var nameFirst = SdmString.CleanAndTrim(dtoSignUp.NameFirst);
        var nameLast = SdmString.CleanAndTrim(dtoSignUp.NameLast);
        var otpId = SdmString.CleanAndTrim(dtoSignUp.OtpId);

        if (!SdmNumber.IsValid(id) ||
            !SdmString.IsValid(id, 8, 8) ||
            !SdmString.IsValid(password, 64) ||
            !SdmString.IsValid(passwordConfirm, 64) ||
            !SdmString.IsValid(nameNick, 256) ||
            !SdmString.IsValid(nameFirst, 256) ||
            !SdmString.IsValid(nameLast, 256) ||
            !SdmString.IsValid(otpId, 64, 64))
        {
            return BadRequest(new { message = "ข้อมูลไม่ถูกต้อง" });
        }

        var idInt = int.Parse(id);
        if (SdmUser.GetBy(idInt) != null)
            return Conflict(new { message = "รหัสผู้ใช้งานถูกสมัครสมาชิกไว้อยู่แล้ว" });

        if (password != passwordConfirm)
            return BadRequest(new { message = "ยืนยันรหัสผ่านไม่ถูกต้อง" });

        if (!SdmAuthentication.IsPasswordStrong(password))
            return BadRequest(new { message = "รหัสผ่านไม่แข็งแรงพอ" });

        var newUser = new User(
            idInt,
            SdmAuthentication.PasswordHash(password),
            nameNick,
            nameFirst,
            nameLast,
            "",
            false,
            null
        );

        if (!SdmUser.Verify(newUser, otpId))
            return BadRequest(new { message = "otpa_id ไม่ถูกต้องหรือหมดอายุหรือไม่ได้รับการยืนยัน" });

        return Ok();
    }
    
    public class DtoSignUpTest
    {
        public required string Id { get; init; } = string.Empty;
        public required string Password { get; init; } = string.Empty;
        public required string PasswordConfirm { get; init; } = string.Empty;
        public required string NameNick { get; init; } = string.Empty;
        public required string NameFirst { get; init; } = string.Empty;
        public required string NameLast { get; init; } = string.Empty;
        public required string OtpId { get; init; } = string.Empty;
    }


    public class DtoToken
    {
        public required string UserTokenId { get; init; } = string.Empty;
    }
    #endregion
}
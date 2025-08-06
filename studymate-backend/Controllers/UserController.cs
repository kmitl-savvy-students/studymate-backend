using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Helper;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    #region [PUT] Update Password
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPut("update/password")]
    public ActionResult<User> UpdatePassword([FromBody] DtoUpdateUserPassword request)
    {
        var existingUser = SdmUser.GetBy(request.Id);
        if (existingUser == null)
            return NotFound(new { message = "User not found." });

        var password = request.Password;
        var passwordConfirm = request.PasswordConfirm;

        if (!SdmString.IsValid(password, 64) ||
            !SdmString.IsValid(passwordConfirm, 64))
            return BadRequest(new { message = "ข้อมูลไม่ถูกต้อง" });

        if (password != passwordConfirm)
            return BadRequest(new { message = "ยืนยันรหัสผ่านไม่ถูกต้อง" });

        if (!SdmAuthentication.IsPasswordStrong(password))
            return BadRequest(new { message = "รหัสผ่านไม่แข็งแรงพอ" });

        existingUser.Password = SdmAuthentication.PasswordHash(request.Password);
        SdmUser.UpdateBy(existingUser);

        return Ok(existingUser);
    }

    public class DtoUpdateUserPassword
    {
        public required int Id { get; init; } = -1;
        public required string Password { get; init; } = string.Empty;
        public required string PasswordConfirm { get; init; } = string.Empty;
    }
    #endregion
    #region [PUT] Update User Info
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPut("update/data")]
    public ActionResult<User> Update([FromBody] DtoUpdateUser user)
    {
        var existingUser = SdmUser.GetBy(user.Id);
        if (existingUser == null)
            return NotFound();

        if (user.NickName.Trim() != "")
            existingUser.Nickname = user.NickName;
        if (user.FirstName.Trim() != "")
            existingUser.Firstname = user.FirstName;
        if (user.LastName.Trim() != "")
            existingUser.Lastname = user.LastName;
        if (user.ProfilePicture.Trim() != "")
            existingUser.ProfilePicture = user.ProfilePicture;

        SdmUser.UpdateBy(existingUser);

        return Ok(user);
    }

    public class DtoUpdateUser
    {
        public required int Id { get; init; } = -1;
        public required string NickName { get; init; } = string.Empty;
        public required string FirstName { get; init; } = string.Empty;
        public required string LastName { get; init; } = string.Empty;
        public required string ProfilePicture { get; init; } = string.Empty;
    }

    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPut("update/curriculum")]
    public ActionResult<User> Update([FromBody] DtoUpdateUserCurriculum user)
    {
        var existingUser = SdmUser.GetBy(user.Id);
        if (existingUser == null)
            return NotFound();

        if (user.CurriculumId != -1)
        {
            var newCurriculum = SdmCurriculum.GetBy(user.CurriculumId);
            if (newCurriculum == null)
                return NotFound();
            existingUser.Curriculum = newCurriculum;
        }

        SdmUser.UpdateBy(existingUser);

        return Ok(user);
    }

    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpGet("get-policy-by-user-id")]
    public IActionResult GetByUserId()
    {
        try
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = SdmUser.GetUserInfoFromToken(token);

            if (user == null) return Unauthorized(new { message = "Invalid or expired token." });

            var policyUser = SdmUser.GetViewPolicy(user.Id);

            return Ok(new { policyViewed = policyUser });
            ;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPost("update/policy")]
    public ActionResult UpdatePolicy([FromBody] DtoUpdateUserPolicy user)
    {
        try
        {
            var existingUser = SdmUser.GetBy(user.Id);
            if (existingUser == null) return NotFound();

            if (existingUser.ViewPolicy == 1) return Conflict(new { message = "User view policy already." });

            SdmUser.UpdateViewPolicy(existingUser);

            return Ok(new { message = "Policy viewed status updated successfully." });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    public class DtoUpdateUserCurriculum
    {
        public required int Id { get; init; } = -1;
        public required int CurriculumId { get; init; } = -1;
    }

    public class DtoUpdateUserPolicy
    {
        public required int Id { get; init; } = -1;
    }
    #endregion
}
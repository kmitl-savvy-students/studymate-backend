using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using studymate_backend.Libraries.Methods;
using System.Text.RegularExpressions;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/otp")]
public class OtpAuthenticationController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("request/{user_id}")]
    public IActionResult RequestOtp(int user_id)
    {
        if (string.IsNullOrWhiteSpace(user_id.ToString()) || user_id.ToString().Length != 8 || !Regex.IsMatch(user_id.ToString(), @"^\d{8}$"))
        {
            return BadRequest("รหัสนักศึกษาต้องเป็นตัวเลข 8 หลักเท่านั้น");
        }

        try
        {
            var otp = SdmOtpAuthentication.RequestOtp(user_id);
            return Ok(new
            {
                id = otp.Id,
                referer = otp.Referer
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "can't generate your otp code.", error = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("verify")]
    public ActionResult Verify([FromBody] DtoVerify verify)
    {
        var activeOtp = SdmOtpAuthentication.VerifyOTP(verify.OtpaId, verify.OtpaCode.ToString());

        if (activeOtp == null)
        {
            return BadRequest(new { message = "รหัส OTP ไม่ถูกต้องหรือหมดอายุ กรุณาลองใหม่อีกครั้งค่ะ" });
        }

        if (activeOtp.Status == "VERIFIED")
        {
            return Conflict(new { message = "VERIFIED already exists." });
        }

        return Ok(new { message = "OTP verified successfully." });
    }

    // [AllowAnonymous]
    // [HttpGet("get")]
    // public ActionResult<IEnumerable<OtpAuthentication>> GetAll()
    // {
    //     return Ok(SdmOtpAuthentication.GetAll());
    // }
    // [AllowAnonymous]
    // [HttpGet("checkId/{otpa_id}")]
    // public ActionResult<IEnumerable<OtpAuthentication>> CheckOtpaId(string otpa_id)
    // {
    //     if (SdmOtpAuthentication.CheckExists(otpa_id))
    //     {
    //         return Conflict(new { message = "OTP is already in use." });
    //     }
    //     return Ok(new { message = "OTP is can use." });
    // }
    //
    // [AllowAnonymous]
    // [HttpGet("active")]
    // public ActionResult<IEnumerable<OtpAuthentication>> GetActiveOtps()
    // {
    //     var activeOtps = SdmOtpAuthentication.GetActiveOtps();
    //
    //     if (activeOtps.Count == 0)
    //     {
    //         return NotFound(new { message = "No active OTPs found." });
    //     }
    //
    //     return Ok(activeOtps);
    // }

}

public class DtoVerify
{
    public required string OtpaId { get; set; }
    public required int OtpaCode { get; set; }
}

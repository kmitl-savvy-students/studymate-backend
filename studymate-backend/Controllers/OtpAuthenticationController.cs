using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/otp")]
public class OtpAuthenticationController : ControllerBase
{
    
    [AllowAnonymous]
    [HttpGet("get")]
    public ActionResult<IEnumerable<OtpAuthentication>> GetAll()
    {
        return Ok(SdmOtpAuthentication.GetAll());
    }
    
    [AllowAnonymous]
    [HttpGet("request/{user_id}")]
    public IActionResult RequestOtp(int user_id)
    {
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
            return StatusCode(500, new { message = "Failed to generate OTP.", error = ex.Message });
        }
    }
    
    [AllowAnonymous]
    [HttpGet("checkId/{otpa_id}")]
    public ActionResult<IEnumerable<OtpAuthentication>> CheckOtpaId(string otpa_id)
    {
        if (SdmOtpAuthentication.CheckExists(otpa_id))
        {
            return Conflict(new { message = "OTP is already in use." });
        }
        return Ok(new { message = "OTP is can use." });
    }
    
    [AllowAnonymous]
    [HttpGet("active")]
    public ActionResult<IEnumerable<OtpAuthentication>> GetActiveOtps()
    {
        var activeOtps = SdmOtpAuthentication.GetActiveOtps();
    
        if (activeOtps.Count == 0)
        {
            return NotFound(new { message = "No active OTPs found." });
        }

        return Ok(activeOtps);
    }

    
}

public class OtpRequest
{
    public string Email { get; set; }
}
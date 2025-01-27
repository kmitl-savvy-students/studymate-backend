using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Helper;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/google")]
public class GoogleOAuthController : ControllerBase
{
    #region Google Environment Variables
    private readonly string _oAuth2Endpoint = Environment.GetEnvironmentVariable("OAUTH2_ENDPOINT") ?? "";
    private readonly string _oAuth2EndpointToken = Environment.GetEnvironmentVariable("OAUTH2_ENDPOINT_TOKEN") ?? "";
    private readonly string _oAuth2EndpointUserInfo = Environment.GetEnvironmentVariable("OAUTH2_ENDPOINT_USER_INFO") ?? "";
    private readonly string _oAuthClientId = Environment.GetEnvironmentVariable("OAUTH_CLIENT_ID") ?? "";
    private readonly string _oAuthClientSecret = Environment.GetEnvironmentVariable("OAUTH_CLIENT_SECRET") ?? "";
    private readonly string _oAuthEndpointUserInfo = Environment.GetEnvironmentVariable("OAUTH_ENDPOINT_USER_INFO") ?? "";
    private readonly string _oAuthRedirectUri = Environment.GetEnvironmentVariable("OAUTH_REDIRECT_URI") ?? "";
    #endregion

    #region [GET] Google Link
    [AllowAnonymous]
    [HttpGet("link/sign-up")]
    public ActionResult GetLinkSignUp()
    {
        return Ok(new { href = GetLink(_oAuthRedirectUri + "/sign-up") });
    }
    [AllowAnonymous]
    [HttpGet("link/sign-in")]
    public ActionResult GetLinkSignIn()
    {
        return Ok(new { href = GetLink(_oAuthRedirectUri + "/sign-in") });
    }

    private string GetLink(string redirectUri)
    {
        var scopes = new[] { _oAuthEndpointUserInfo + ".email", _oAuthEndpointUserInfo + ".profile" };

        return $"{_oAuth2Endpoint}?client_id={Uri.EscapeDataString(_oAuthClientId)}" +
               $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
               "&response_type=code" +
               $"&scope={Uri.EscapeDataString(string.Join(" ", scopes))}" +
               "&access_type=offline" +
               "&prompt=consent";
    }
    #endregion
    #region [POST] Google Callback
    [AllowAnonymous]
    [HttpPost("callback")]
    public async Task<ActionResult<UserToken>> Callback(DtoGoogleCallback callback)
    {
        var authorizationCode = SdmString.CleanAndTrim(callback.Code);

        var googleAccessToken =
            await GetAccessTokenAsync(authorizationCode, _oAuthRedirectUri + "/" + callback.RedirectUri);
        if (googleAccessToken == null || string.IsNullOrEmpty(googleAccessToken.AccessToken))
            return Unauthorized(new { message = "ไม่สามารถเข้าสู่ระบบหรือสมัครสมาชิกด้วย Google ได้" });

        var client = new HttpClient();
        var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, _oAuth2EndpointUserInfo);
        userInfoRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", googleAccessToken.AccessToken);

        var response = await client.SendAsync(userInfoRequest);
        if (!response.IsSuccessStatusCode)
            return Unauthorized(new { message = "ไม่สามารถเข้าถึงข้อมูลผู้ใช้ได้" });

        var responseContent = await response.Content.ReadAsStringAsync();
        var userInfo = JsonSerializer.Deserialize<DtoUserInfo>(responseContent);

        if (userInfo == null)
            return Unauthorized(new { message = "ไม่สามารถเข้าถึงข้อมูลผู้ใช้ได้" });

        var id = userInfo.Email.Split('@')[0];
        var domain = userInfo.Hd;

        if (domain != "kmitl.ac.th" || !SdmNumber.IsValid(id) || !SdmString.IsValid(id, 8, 8))
            return Unauthorized(new { message = "กรุณาใช้บัญชีของสถาบันเท่านั้น" });

        var idInt = int.Parse(id);

        var user = SdmUser.GetBy(idInt);
        if (user == null)
        {
            if (callback.RedirectUri == "sign-in")
                return NotFound(new { message = "ไม่พบข้อมูลผู้ใช้งาน กรุณาสมัครสมาชิก" });

            user = new User(
                idInt,
                SdmAuthentication.PasswordHash(SdmString.GenerateRandomToken()),
                userInfo.GivenName,
                userInfo.GivenName,
                userInfo.FamilyName,
                userInfo.Picture,
                false,
                null
            );
            SdmUser.Insert(user);
        }
        else
        {
            if (callback.RedirectUri == "sign-up")
                return Conflict(new { message = "คุณได้สมัครสมาชิกไปแล้ว กรุณาเข้าสู่ระบบแทน" });

            user.NameFirst = userInfo.GivenName;
            user.NameLast = userInfo.FamilyName;
            user.Profile = userInfo.Picture;
            SdmUser.UpdateBy(user);
        }

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

    public class DtoUserInfo
    {
        public required string Id { get; init; }
        public required string Email { get; init; }
        public required bool VerifiedEmail { get; init; }
        public required string Name { get; init; }
        public required string GivenName { get; init; }
        public required string FamilyName { get; init; }
        public required string Picture { get; init; }
        public required string Hd { get; init; }
    }

    public class DtoGoogleCallback
    {
        public required string Code { get; init; } = string.Empty;
        public required string RedirectUri { get; init; } = string.Empty;
    }

    private async Task<DtoGoogleAccessToken?> GetAccessTokenAsync(string code, string redirectUri)
    {
        var client = new HttpClient();
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, _oAuth2EndpointToken)
        {
            Content = new FormUrlEncodedContent([
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", _oAuthClientId),
                new KeyValuePair<string, string>("client_secret", _oAuthClientSecret),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("grant_type", "authorization_code")
            ])
        };

        var response = await client.SendAsync(tokenRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<DtoGoogleAccessToken>(responseContent);
    }

    public class DtoGoogleAccessToken
    {
        public string? AccessToken { get; init; }
        public int? ExpiresIn { get; init; }
        public string? RefreshToken { get; init; }
        public string? Scope { get; init; }
        public string? TokenType { get; init; }
        public string? IdToken { get; init; }
    }
    #endregion
}
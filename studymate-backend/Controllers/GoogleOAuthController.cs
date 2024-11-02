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
    private readonly string _oAuth2Endpoint = Environment.GetEnvironmentVariable("OAUTH2_ENDPOINT") ?? "";
    private readonly string _oAuth2EndpointToken = Environment.GetEnvironmentVariable("OAUTH2_ENDPOINT_TOKEN") ?? "";
    private readonly string _oAuth2EndpointUserInfo = Environment.GetEnvironmentVariable("OAUTH2_ENDPOINT_USER_INFO") ?? "";
    private readonly string _oAuthClientId = Environment.GetEnvironmentVariable("OAUTH_CLIENT_ID") ?? "";
    private readonly string _oAuthClientSecret = Environment.GetEnvironmentVariable("OAUTH_CLIENT_SECRET") ?? "";
    private readonly string _oAuthEndpointUserInfo = Environment.GetEnvironmentVariable("OAUTH_ENDPOINT_USER_INFO") ?? "";
    private readonly string _oAuthRedirectUri = Environment.GetEnvironmentVariable("OAUTH_REDIRECT_URI") ?? "";

    [AllowAnonymous]
    [HttpGet("link/sign-up")]
    public ActionResult<DtoGoogleLink> GetLinkSignUp()
    {
        return Ok(new DtoGoogleLink { href = GetLink(_oAuthRedirectUri + "/sign-up") });
    }

    [AllowAnonymous]
    [HttpGet("link/sign-in")]
    public ActionResult<DtoGoogleLink> GetLinkSignIn()
    {
        return Ok(new DtoGoogleLink { href = GetLink(_oAuthRedirectUri + "/sign-in") });
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

    [AllowAnonymous]
    [HttpPost("callback")]
    public async Task<ActionResult<UserToken>> Callback(DtoGoogleCallback callback)
    {
        var authorizationCode = SdmString.CleanAndTrim(callback.code);

        // Get Access Token
        var googleAccessToken = await GetAccessTokenAsync(authorizationCode, _oAuthRedirectUri + "/" + callback.redirectUri);
        if (googleAccessToken == null || string.IsNullOrEmpty(googleAccessToken.access_token))
            return Unauthorized("Cannot get Google access token.");

        // Get User Info from Access Token
        var client = new HttpClient();
        var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, _oAuth2EndpointUserInfo);
        userInfoRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", googleAccessToken.access_token);

        var response = await client.SendAsync(userInfoRequest);
        if (!response.IsSuccessStatusCode)
            return Unauthorized("Cannot get user info.");

        var responseContent = await response.Content.ReadAsStringAsync();
        var userInfo = JsonSerializer.Deserialize<DtoUserInfo>(responseContent);

        if (userInfo == null)
            return Unauthorized("Cannot get user info");

        var id = (userInfo.email ?? "00000000@kmitl.ac.th").Split('@')[0];
        var domain = userInfo.hd;

        // Verify KMITL user
        if (domain != "kmitl.ac.th" || !SdmNumber.IsValid(id) || !SdmString.IsValid(id, 8, 8))
            return Unauthorized("Must use KMITL Account.");

        var user = SdmUser.GetBy(id);
        if (user == null)
        {
            if (callback.redirectUri == "sign-in")
                return NotFound("User not found, please sign up.");

            // Create user if user doesn't exist
            user = new User(
                id,
                SdmAuthentication.PasswordHash(SdmString.GenerateRandomToken()),
                userInfo.given_name,
                userInfo.given_name,
                userInfo.family_name,
                userInfo.picture,
                null
            );
            SdmUser.Insert(user);
        }
        else
        {
            if (callback.redirectUri == "sign-up")
                return Conflict("You already sign up, please sign in.");

            user.nameFirst = userInfo.given_name;
            user.nameLast = userInfo.family_name;
            user.profile = userInfo.picture;
            SdmUser.Update(user);
        }

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

    private async Task<DtoGoogleAccessToken?> GetAccessTokenAsync(string code, string redirectUri)
    {
        var client = new HttpClient();
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, _oAuth2EndpointToken)
        {
            Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", _oAuthClientId),
                new KeyValuePair<string, string>("client_secret", _oAuthClientSecret),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("grant_type", "authorization_code")
            })
        };

        var response = await client.SendAsync(tokenRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<DtoGoogleAccessToken>(responseContent);
    }

    public class DtoGoogleCallback
    {
        public required string code { get; set; }
        public required string redirectUri { get; set; }
    }

    public class DtoGoogleAccessToken
    {
        public required string access_token { get; set; }
        public required int expires_in { get; set; }
        public required string refresh_token { get; set; }
        public required string scope { get; set; }
        public required string token_type { get; set; }
        public required string id_token { get; set; }
    }

    public class DtoUserInfo
    {
        public required string id { get; set; }
        public required string email { get; set; }
        public required bool verified_email { get; set; }
        public required string name { get; set; }
        public required string given_name { get; set; }
        public required string family_name { get; set; }
        public required string picture { get; set; }
        public required string hd { get; set; }
    }

    public class DtoGoogleLink
    {
        public required string href { get; set; }
    }
}

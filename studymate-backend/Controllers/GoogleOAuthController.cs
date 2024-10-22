using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Controllers.Core;
using studymate_backend.Enums;
using studymate_backend.Helper;
using studymate_backend.Models.Core;
using studymate_backend.Models.StudyMate.Object;
using studymate_backend.Models.StudyMate.Raw.Request.Auth;
using studymate_backend.Services;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/google")]
public class GoogleOAuthController(
    UserService userService,
    UserTokenService userTokenService
) : IController
{
    private readonly string oAuthClientId = Environment.GetEnvironmentVariable("OAUTH_CLIENT_ID") ?? "";
    private readonly string oAuthClientSecret = Environment.GetEnvironmentVariable("OAUTH_CLIENT_SECRET") ?? "";
    private readonly string oAuthRedirectUri = Environment.GetEnvironmentVariable("OAUTH_REDIRECT_URI") ?? "";
    private readonly string oAuthEndpointUserInfo = Environment.GetEnvironmentVariable("OAUTH_ENDPOINT_USER_INFO") ?? "";
    
    private readonly string oAuth2Endpoint = Environment.GetEnvironmentVariable("OAUTH2_ENDPOINT") ?? "";
    private readonly string oAuth2EndpointToken = Environment.GetEnvironmentVariable("OAUTH2_ENDPOINT_TOKEN") ?? "";
    private readonly string oAuth2EndpointUserInfo = Environment.GetEnvironmentVariable("OAUTH2_ENDPOINT_USER_INFO") ?? "";
    

    [HttpGet("link/sign-in")]
    public BaseResponse GetLinkSignIn()
    {
        return new BaseResponse(EnumResponseCode.OK, GetLink(oAuthRedirectUri + "/sign-in"));
    }

    [HttpGet("link/sign-up")]
    public BaseResponse GetLinkSignUp()
    {
        return new BaseResponse(EnumResponseCode.OK, GetLink(oAuthRedirectUri + "/sign-up"));
    }

    private string GetLink(string redirectUri)
    {
        var scopes = new[] { oAuthEndpointUserInfo + ".email", oAuthEndpointUserInfo + ".profile" };

        return $"{oAuth2Endpoint}?client_id={Uri.EscapeDataString(oAuthClientId)}" +
               $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
               "&response_type=code" +
               $"&scope={Uri.EscapeDataString(string.Join(" ", scopes))}" +
               "&access_type=offline" +
               "&prompt=consent";
    }

    [HttpPost("callback")]
    public async Task<BaseResponse> PostCallback(RequestGoogleCallback request)
    {
        var authorizationCode = SdmString.cleanAndTrim(request.Code);

        try
        {
            // Get Access Token
            var googleAccessToken = await GetAccessTokenAsync(authorizationCode, oAuthRedirectUri + "/sign-in");
            if (googleAccessToken == null || string.IsNullOrEmpty(googleAccessToken.access_token))
                return new BaseResponse(EnumResponseCode.UNAUTHORIZED);

            // Get User Info from Access Token
            var client = new HttpClient();
            var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, oAuth2EndpointUserInfo);
            userInfoRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", googleAccessToken.access_token);

            var response = await client.SendAsync(userInfoRequest);
            if (!response.IsSuccessStatusCode)
                return new BaseResponse(EnumResponseCode.UNAUTHORIZED);

            var responseContent = await response.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<UserInfo>(responseContent);

            if (userInfo == null)
                return new BaseResponse(EnumResponseCode.UNAUTHORIZED);

            var id = (userInfo.email ?? "00000000@kmitl.ac.th").Split('@')[0];
            var domain = userInfo.hd;

            // Verify KMITL user
            if (domain != "kmitl.ac.th" || !SdmNumber.IsValid(id) || !SdmString.IsValid(id, 8, 8))
                return new BaseResponse(EnumResponseCode.UNAUTHORIZED);

            var user = userService.Get(id);
            if (user == null)
            {
                // Create user if user doesn't exist
                user = new User(
                    id,
                    SdmAuthentication.passwordHash(SdmString.generateRandomToken()),
                    EnumGender.OTHER,
                    userInfo.given_name ?? "",
                    userInfo.given_name ?? "",
                    userInfo.family_name ?? "",
                    userInfo.picture ?? ""
                );
                userService.Add(user);
            }
            else
            {
                user.NameFirst = userInfo.given_name ?? user.NameFirst;
                user.NameLast = userInfo.family_name ?? user.NameLast;
                user.Profile = userInfo.picture ?? user.Profile;
                userService.Update(user);
            }

            // Generate token string
            var randomizeToken = SdmString.generateRandomToken();
            while (userTokenService.Get(randomizeToken) != null)
                randomizeToken = SdmString.generateRandomToken();

            // Verify if token is already exists
            var userToken = userTokenService.GetByUser(user);
            if (userToken != null)
                userTokenService.Remove(userToken);

            // Create token
            userToken = new UserToken(
                randomizeToken,
                user,
                SdmDateTime.Now(),
                SdmDateTime.Now().AddHours(12)
            );
            userTokenService.Add(userToken);

            return new BaseResponse(EnumResponseCode.CREATED, userToken.Serialized());
        }
        catch (Exception ex)
        {
            return new BaseResponse(EnumResponseCode.INTERNAL_SERVER_ERROR, ex.Message);
        }
    }

    private async Task<GoogleAccessToken?> GetAccessTokenAsync(string code, string redirectUri)
    {
        var client = new HttpClient();
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, oAuth2EndpointToken)
        {
            Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", oAuthClientId),
                new KeyValuePair<string, string>("client_secret", oAuthClientSecret),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("grant_type", "authorization_code")
            })
        };

        var response = await client.SendAsync(tokenRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GoogleAccessToken>(responseContent);
    }
}

public class GoogleAccessToken
{
    public string? access_token { get; set; }
    public int? expires_in { get; set; }
    public string? refresh_token { get; set; }
    public string? scope { get; set; }
    public string? token_type { get; set; }
    public string? id_token { get; set; }
}

public class UserInfo
{
    public string? id { get; set; }
    public string? email { get; set; }
    public bool? verified_email { get; set; }
    public string? name { get; set; }
    public string? given_name { get; set; }
    public string? family_name { get; set; }
    public string? picture { get; set; }
    public string? hd { get; set; }
}
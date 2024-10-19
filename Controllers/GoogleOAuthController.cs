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
using studymate_backend.Services.FrontendUrl;
using studymate_backend.Services.GoogleOAuthUrl;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/google")]
public class GoogleOAuthController(
    UserService userService,
    UserTokenService userTokenService,
    IFrontendUrlService frontendUrlService,
    IGoogleOAuthUrlService googleOAuthUrlService
) : IController
{
    private const string clientId = "119650545901-2l5s16n5j047nsqb09uj86focr7jkvk5.apps.googleusercontent.com";
    private const string clientSecret = "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";

    [HttpGet("link/sign-in")]
    public BaseResponse GetLinkSignIn()
    {
        return new BaseResponse(EnumResponseCode.OK, GetLink(frontendUrlService.GetFrontendUrl() + "/sign-in"));
    }

    [HttpGet("link/sign-up")]
    public BaseResponse GetLinkSignUp()
    {
        return new BaseResponse(EnumResponseCode.OK, GetLink(frontendUrlService.GetFrontendUrl() + "/sign-up"));
    }

    public string GetLink(string redirectUri)
    {
        var authEndpoint = googleOAuthUrlService.GetAuthEndpoint();
        var scopes = new[] { googleOAuthUrlService.GetUserInfoEndpoint() + ".email", googleOAuthUrlService.GetUserInfoEndpoint() + ".profile" };

        return $"{authEndpoint}?client_id={Uri.EscapeDataString(clientId)}" +
               $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
               "&response_type=code" +
               $"&scope={Uri.EscapeDataString(string.Join(" ", scopes))}" +
               "&access_type=offline" +
               "&prompt=consent";
    }

    [HttpPost("callback")]
    public async Task<BaseResponse> PostCallback(RequestGoogleCallback request)
    {
        var authorizationCode = SDMString.cleanAndTrim(request.Code);

        try
        {
            // Get Access Token
            var googleAccessToken = await GetAccessTokenAsync(authorizationCode, frontendUrlService.GetFrontendUrl() + "/sign-in");
            if (googleAccessToken == null || string.IsNullOrEmpty(googleAccessToken.access_token))
                return new BaseResponse(EnumResponseCode.UNAUTHORIZED);

            // Get User Info from Access Token
            var client = new HttpClient();
            var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, googleOAuthUrlService.GetUserInfoOAuth2Endpoint());
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
            if (domain != "kmitl.ac.th" || !SDMNumber.IsValid(id) || !SDMString.IsValid(id, 8, 8))
                return new BaseResponse(EnumResponseCode.UNAUTHORIZED);

            var user = userService.Get(id);
            if (user == null)
            {
                // Create user if user doesn't exist
                user = new User(
                    id,
                    SDMAuthentication.passwordHash(SDMString.generateRandomToken()),
                    EnumGender.OTHER,
                    userInfo.given_name ?? "",
                    userInfo.given_name ?? "",
                    userInfo.family_name ?? ""
                );
                userService.Add(user);
            }

            // Generate token string
            var randomizeToken = SDMString.generateRandomToken();
            while (userTokenService.Get(randomizeToken) != null)
                randomizeToken = SDMString.generateRandomToken();

            // Verify if token is already exists
            var userToken = userTokenService.GetByUser(user);
            if (userToken != null)
                userTokenService.Remove(userToken);

            // Create token
            userToken = new UserToken(
                randomizeToken,
                user,
                SDMDateTime.Now(),
                SDMDateTime.Now().AddHours(12)
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
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, googleOAuthUrlService.GetOAuthTokenEndpoint())
        {
            Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
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
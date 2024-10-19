namespace studymate_backend.Services.GoogleOAuthUrl;

public class GoogleOAuthConfig
{
    public required string UserInfoEndpoint { get; set; }
    public required string UserInfoOAuth2Endpoint { get; set; }
    public required string OAuthTokenEndpoint { get; set; }
    public required string AuthEndpoint { get; set; }
}
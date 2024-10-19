namespace studymate_backend.Services.GoogleOAuthUrl;

public interface IGoogleOAuthUrlService
{
    string GetUserInfoEndpoint();
    string GetOAuthTokenEndpoint();
    string GetAuthEndpoint();
    string GetUserInfoOAuth2Endpoint();
}
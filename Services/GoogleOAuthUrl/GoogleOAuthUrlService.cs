using Microsoft.Extensions.Options;

namespace studymate_backend.Services.GoogleOAuthUrl;

public class GoogleOAuthUrlService(IOptions<GoogleOAuthConfig> config) : IGoogleOAuthUrlService
{
    private readonly GoogleOAuthConfig _config = config.Value;

    public string GetUserInfoEndpoint()
    {
        return _config.UserInfoEndpoint;
    }

    public string GetOAuthTokenEndpoint()
    {
        return _config.OAuthTokenEndpoint;
    }

    public string GetAuthEndpoint()
    {
        return _config.AuthEndpoint;
    }

    public string GetUserInfoOAuth2Endpoint()
    {
        return _config.UserInfoOAuth2Endpoint;
    }
}
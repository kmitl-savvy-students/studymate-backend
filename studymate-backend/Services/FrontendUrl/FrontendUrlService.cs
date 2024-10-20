using Microsoft.Extensions.Options;

namespace studymate_backend.Services.FrontendUrl;

public class FrontendUrlService(IOptions<FrontendConfig> config) : IFrontendUrlService
{
    private readonly FrontendConfig _config = config.Value;


    public string? GetFrontendUrl()
    {
        return _config.FrontendUrl;
    }
}
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using studymate_backend.Libraries.Methods;

namespace studymate_backend.Libraries.Core;

public class SdmTokenHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder
)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var endpoint = Context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
        {
            Logger.LogInformation("Skipping authentication for an anonymous endpoint.");
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!Request.Headers.ContainsKey("Authorization"))
        {
            Logger.LogWarning("Authorization header is missing in the request.");
            return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));
        }

        var token = Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(token))
        {
            Logger.LogWarning("Authorization header is empty.");
            return Task.FromResult(AuthenticateResult.Fail("Authorization header is empty"));
        }

        token = token.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);

        var userToken = SdmUserToken.GetBy(token);
        if (userToken == null)
        {
            Logger.LogWarning("Invalid or expired token.");
            return Task.FromResult(AuthenticateResult.Fail("Invalid or expired token"));
        }

        var user = userToken.user;
        if (user == null)
        {
            Logger.LogWarning("User not found for the provided token.");
            return Task.FromResult(AuthenticateResult.Fail("User not found"));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.GetFullName()),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
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
        if (!Request.Headers.ContainsKey("Authorization"))
            return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));

        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");

        var userToken = SdmUserToken.GetBy(token);
        if (userToken == null)
            return Task.FromResult(AuthenticateResult.Fail("Invalid or expired token"));

        var user = userToken.user;
        if (user == null)
            return Task.FromResult(AuthenticateResult.Fail("User not found"));

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.GetFullName()),
            new Claim(ClaimTypes.NameIdentifier, user.id)
        };

        var identity = new ClaimsIdentity(claims, nameof(SdmTokenHandler));
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
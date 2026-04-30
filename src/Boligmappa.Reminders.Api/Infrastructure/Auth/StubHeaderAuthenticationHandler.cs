using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Boligmappa.Reminders.Api.Infrastructure.Auth;

/// <summary>
/// Take-home stub: trusts an "X-User-Id: {guid}" request header and emits a
/// ClaimsPrincipal with NameIdentifier = guid. The shape of the auth pipeline
/// (authentication scheme -> authorization policies -> resource handlers) is
/// the production design; only the credential mechanism is simplified.
///
/// In production: replace with JWT bearer / cookie / OIDC. The downstream
/// authorization handlers do not need to change.
/// </summary>
public sealed class StubHeaderAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(AuthSchemes.HeaderName, out var values) || values.Count == 0)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!Guid.TryParse(values.ToString(), out var userId))
        {
            return Task.FromResult(AuthenticateResult.Fail($"{AuthSchemes.HeaderName} is not a valid Guid."));
        }

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

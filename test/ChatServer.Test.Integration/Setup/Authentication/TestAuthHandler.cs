using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChatServer.Test.Integration.Setup.Authentication;

public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory loggerFactory,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, loggerFactory, encoder)
{
    /// <summary>
    /// Header a test can set to control the authenticated tenant/client id.
    /// The value is surfaced as the <see cref="ClaimTypes.NameIdentifier"/> claim,
    /// which is what <c>ClaimsPrincipal.GetUserId()</c> reads.
    /// </summary>
    public const string TenantIdHeader = "X-Test-TenantId";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "test_user"),
            new(ClaimTypes.Role, "admin")
        };

        if (Request.Headers.TryGetValue(TenantIdHeader, out var tenantId) &&
            !string.IsNullOrWhiteSpace(tenantId))
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, tenantId.ToString()));
        }

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

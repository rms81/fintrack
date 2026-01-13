using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinTrack.Tests.Integration.Auth;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestScheme";
    public const string DefaultUserId = "test-user-id";
    public const string DefaultUserEmail = "test@example.com";
    public const string DefaultUserName = "Test User";

    public static string? TestUserId { get; set; } = DefaultUserId;
    public static string? TestUserEmail { get; set; } = DefaultUserEmail;
    public static string? TestUserName { get; set; } = DefaultUserName;
    public static bool IsAuthenticated { get; set; } = true;

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!IsAuthenticated)
        {
            return Task.FromResult(AuthenticateResult.Fail("Not authenticated"));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, TestUserId ?? DefaultUserId),
            new("sub", TestUserId ?? DefaultUserId),
            new(ClaimTypes.Email, TestUserEmail ?? DefaultUserEmail),
            new("email", TestUserEmail ?? DefaultUserEmail),
            new(ClaimTypes.Name, TestUserName ?? DefaultUserName),
            new("name", TestUserName ?? DefaultUserName)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    public static void Reset()
    {
        TestUserId = DefaultUserId;
        TestUserEmail = DefaultUserEmail;
        TestUserName = DefaultUserName;
        IsAuthenticated = true;
    }
}

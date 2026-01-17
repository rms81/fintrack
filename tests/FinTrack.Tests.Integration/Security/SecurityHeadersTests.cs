using System.Net;

namespace FinTrack.Tests.Integration.Security;

public class SecurityHeadersTests : IClassFixture<FinTrackWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SecurityHeadersTests(FinTrackWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Request_IncludesSecurityHeaders()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Verify all security headers are present
        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").First());

        Assert.True(response.Headers.Contains("X-Frame-Options"));
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").First());

        Assert.True(response.Headers.Contains("Referrer-Policy"));
        Assert.Equal("strict-origin-when-cross-origin", response.Headers.GetValues("Referrer-Policy").First());

        Assert.True(response.Headers.Contains("Permissions-Policy"));
        Assert.Equal("geolocation=(), microphone=(), camera=()", 
            response.Headers.GetValues("Permissions-Policy").First());
    }

    [Fact]
    public async Task Request_IncludesContentSecurityPolicy()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("Content-Security-Policy"));
        
        var csp = response.Headers.GetValues("Content-Security-Policy").First();
        
        // Verify CSP is set and contains expected directives
        Assert.NotEmpty(csp);
        Assert.Contains("default-src 'self'", csp);
        Assert.Contains("script-src", csp);
        Assert.Contains("style-src", csp);
        Assert.Contains("img-src 'self' data:", csp);
        Assert.Contains("connect-src", csp);
        Assert.Contains("font-src 'self' data:", csp);
    }

    [Fact]
    public async Task TestEnvironment_CSP_DoesNotIncludeUnsafeEval()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        var csp = response.Headers.GetValues("Content-Security-Policy").First();
        
        // Test environment should not include 'unsafe-eval'
        Assert.DoesNotContain("unsafe-eval", csp);
    }

    [Fact]
    public async Task TestEnvironment_CSP_DoesNotIncludeUnsafeInlineForStyles()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        var csp = response.Headers.GetValues("Content-Security-Policy").First();
        
        // Extract style-src directive
        var styleSrcMatch = System.Text.RegularExpressions.Regex.Match(csp, @"style-src[^;]+");
        Assert.True(styleSrcMatch.Success);
        
        var styleSrc = styleSrcMatch.Value;
        
        // Should not include 'unsafe-inline' for styles
        Assert.DoesNotContain("unsafe-inline", styleSrc);
    }

    [Fact]
    public async Task ApiEndpoint_IncludesSecurityHeaders()
    {
        // Act
        var response = await _client.GetAsync("/api/profiles");

        // Assert - Even API endpoints should have security headers
        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
        Assert.True(response.Headers.Contains("X-Frame-Options"));
        Assert.True(response.Headers.Contains("Content-Security-Policy"));
    }
}

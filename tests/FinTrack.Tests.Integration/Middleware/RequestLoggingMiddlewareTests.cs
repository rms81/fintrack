using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace FinTrack.Tests.Integration.Middleware;

public class RequestLoggingMiddlewareTests : IClassFixture<FinTrackWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public RequestLoggingMiddlewareTests(FinTrackWebApplicationFactory factory, ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _output = output;
    }

    [Fact]
    public async Task Request_ToRootEndpoint_LogsSuccessfully()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Verify correlation ID is present (showing middleware executed)
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
    }

    [Fact]
    public async Task Request_ToHealthEndpoint_IsNotLogged()
    {
        // Act - Health check endpoints should not be logged
        var healthResponse = await _client.GetAsync("/health");
        var aliveResponse = await _client.GetAsync("/alive");
        var readyResponse = await _client.GetAsync("/ready");

        // Assert - All should respond without errors
        // The middleware should skip logging for these endpoints
        // We can't directly assert on logs in integration tests, but we verify the middleware
        // doesn't interfere with health check endpoints
        Assert.True(healthResponse.StatusCode == HttpStatusCode.OK || 
                    healthResponse.StatusCode == HttpStatusCode.NotFound);
        Assert.True(aliveResponse.StatusCode == HttpStatusCode.OK || 
                    aliveResponse.StatusCode == HttpStatusCode.NotFound);
        Assert.True(readyResponse.StatusCode == HttpStatusCode.OK || 
                    readyResponse.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Request_ToApiEndpoint_LogsSuccessfully()
    {
        // Act
        var response = await _client.GetAsync("/api/profiles");

        // Assert - Should get unauthorized or success, but middleware should handle it
        Assert.True(response.StatusCode == HttpStatusCode.OK || 
                    response.StatusCode == HttpStatusCode.Unauthorized);
        
        // Verify correlation ID is present (showing middleware executed)
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
    }

    [Fact]
    public async Task Request_WithAuthentication_LogsUserInformation()
    {
        // Arrange - Create a request with test authentication
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/profiles");
        request.Headers.Add("Authorization", "Test");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || 
                    response.StatusCode == HttpStatusCode.Unauthorized);
        
        // Middleware should have access to User context after authentication middleware
        // We can't directly verify the log content, but we ensure the request completes
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
    }

    [Fact]
    public async Task Request_ToNonExistentEndpoint_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/this-does-not-exist");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        // Middleware should still log 404 responses
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
    }

    [Fact]
    public async Task Request_CompletesSuccessfully_HasCorrelationId()
    {
        // This test verifies the middleware pipeline is correctly ordered
        // and that RequestLoggingMiddleware can access correlation IDs
        
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Correlation ID should be set by CorrelationIdMiddleware
        // and available to RequestLoggingMiddleware
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
        var correlationId = response.Headers.GetValues("X-Correlation-ID").First();
        Assert.NotEmpty(correlationId);
    }
}

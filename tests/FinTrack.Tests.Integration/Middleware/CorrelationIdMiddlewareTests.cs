using System.Net;

namespace FinTrack.Tests.Integration.Middleware;

public class CorrelationIdMiddlewareTests : IClassFixture<FinTrackWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CorrelationIdMiddlewareTests(FinTrackWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Request_WithoutCorrelationIdHeader_GeneratesCorrelationId()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
        
        var correlationId = response.Headers.GetValues("X-Correlation-ID").First();
        Assert.NotEmpty(correlationId);
        
        // Should be a valid format (32 chars for GUID without dashes or trace ID)
        Assert.True(correlationId.Length >= 32);
    }

    [Fact]
    public async Task Request_WithCorrelationIdHeader_PreservesCorrelationId()
    {
        // Arrange
        var expectedCorrelationId = "test-correlation-id-12345";
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add("X-Correlation-ID", expectedCorrelationId);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
        
        var correlationId = response.Headers.GetValues("X-Correlation-ID").First();
        Assert.Equal(expectedCorrelationId, correlationId);
    }

    [Fact]
    public async Task Request_WithInvalidCorrelationIdHeader_GeneratesNewCorrelationId()
    {
        // Arrange - correlation ID with invalid characters
        var invalidCorrelationId = "test@correlation#id!";
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add("X-Correlation-ID", invalidCorrelationId);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
        
        var correlationId = response.Headers.GetValues("X-Correlation-ID").First();
        
        // Should have generated a new one, not used the invalid one
        Assert.NotEqual(invalidCorrelationId, correlationId);
        Assert.True(correlationId.Length >= 32);
    }

    [Fact]
    public async Task Request_WithExcessivelyLongCorrelationIdHeader_GeneratesNewCorrelationId()
    {
        // Arrange - correlation ID that exceeds 128 character limit
        var longCorrelationId = new string('a', 150);
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add("X-Correlation-ID", longCorrelationId);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
        
        var correlationId = response.Headers.GetValues("X-Correlation-ID").First();
        
        // Should have generated a new one, not used the excessively long one
        Assert.NotEqual(longCorrelationId, correlationId);
        Assert.True(correlationId.Length <= 128);
    }

    [Fact]
    public async Task Request_CorrelationIdAddedToResponseHeaders()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
        
        var correlationId = response.Headers.GetValues("X-Correlation-ID").First();
        Assert.NotEmpty(correlationId);
    }

    [Fact]
    public async Task ApiEndpoint_AlsoHasCorrelationId()
    {
        // Act
        var response = await _client.GetAsync("/api/profiles");

        // Assert - Even API endpoints should have correlation IDs
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
        
        var correlationId = response.Headers.GetValues("X-Correlation-ID").First();
        Assert.NotEmpty(correlationId);
    }
}

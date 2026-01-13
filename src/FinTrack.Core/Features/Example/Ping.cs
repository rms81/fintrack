namespace FinTrack.Core.Features.Example;

// Example query
public record PingQuery;

public record PingResponse(string Message, DateTime Timestamp);

// Wolverine handler - follows naming convention
public static class PingQueryHandler
{
    public static PingResponse Handle(PingQuery query) =>
        new("Pong", DateTime.UtcNow);
}

namespace FinTrack.Core.Features.Example;

// Example command
public record EchoCommand(string Message);

public record EchoResponse(string Echo, DateTime Timestamp);

// Wolverine handler
public static class EchoCommandHandler
{
    public static EchoResponse Handle(EchoCommand command) =>
        new($"Echo: {command.Message}", DateTime.UtcNow);
}

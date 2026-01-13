using FinTrack.Core.Features.Example;
using Wolverine.Http;

namespace FinTrack.Host.Endpoints;

public static class ExampleEndpoints
{
    [WolverineGet("/api/ping")]
    public static PingResponse Ping() => PingQueryHandler.Handle(new PingQuery());

    [WolverinePost("/api/echo")]
    public static EchoResponse Echo(EchoCommand command) => EchoCommandHandler.Handle(command);
}

using FinTrack.Core.Features.Example;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace FinTrack.Host.Endpoints;

/// <summary>
/// Example endpoints for testing the API.
/// </summary>
public static class ExampleEndpoints
{
    [WolverineGet("/api/ping")]
    [Tags("System")]
    [EndpointSummary("Ping the API")]
    [EndpointDescription("Returns a simple pong response with the current server timestamp. Use this to verify the API is responding.")]
    [ProducesResponseType<PingResponse>(StatusCodes.Status200OK)]
    public static PingResponse Ping() => PingQueryHandler.Handle(new PingQuery());

    [WolverinePost("/api/echo")]
    [Tags("System")]
    [EndpointSummary("Echo a message")]
    [EndpointDescription("Returns the provided message back to the caller. Useful for testing request/response cycles.")]
    [ProducesResponseType<EchoResponse>(StatusCodes.Status200OK)]
    public static EchoResponse Echo(EchoCommand command) => EchoCommandHandler.Handle(command);
}

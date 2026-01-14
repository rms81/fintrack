namespace FinTrack.Infrastructure.Options;

public class LlmOptions
{
    public const string Section = "Llm";

    public required string ApiKey { get; init; }
    public string Model { get; init; } = "openai/gpt-4-turbo";
    public string BaseUrl { get; init; } = "https://openrouter.ai/api/v1";
    public int MaxTokens { get; init; } = 1000;
    public float Temperature { get; init; } = 0.1f;
}

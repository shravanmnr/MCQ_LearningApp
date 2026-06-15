namespace MCQ_LearningApp.Infrastructure.Options;

public class AnthropicOptions
{
    public const string SectionName = "Anthropic";

    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "anthropic/claude-3-5-haiku";
    public int MaxTokens { get; set; } = 1024;
    public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1";

    // OpenRouter: name shown in your dashboard usage logs
    public string AppName { get; set; } = "ASP.NET MCQ Trainer";
}

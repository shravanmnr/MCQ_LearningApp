using System.Text.Json.Serialization;

namespace MCQ_LearningApp.Infrastructure.Dto;

// OpenAI-compatible chat completions request (used by OpenRouter)
public class AnthropicRequestDto
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<MessageDto> Messages { get; set; } = new();

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; }
}

public class MessageDto
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

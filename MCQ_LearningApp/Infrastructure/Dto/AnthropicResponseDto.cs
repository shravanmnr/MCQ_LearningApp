using System.Text.Json.Serialization;

namespace MCQ_LearningApp.Infrastructure.Dto;

// OpenAI-compatible chat completions response (used by OpenRouter)
public class AnthropicResponseDto
{
    [JsonPropertyName("choices")]
    public List<ChoiceDto> Choices { get; set; } = new();
}

public class ChoiceDto
{
    [JsonPropertyName("message")]
    public MessageDto Message { get; set; } = new();
}

public class ContentBlockDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

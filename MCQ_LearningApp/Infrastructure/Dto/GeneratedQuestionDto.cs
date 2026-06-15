using System.Text.Json.Serialization;

namespace MCQ_LearningApp.Infrastructure.Dto;

public class GeneratedQuestionDto
{
    [JsonPropertyName("question")]
    public string Question { get; set; } = string.Empty;

    [JsonPropertyName("choices")]
    public List<string> Choices { get; set; } = new();

    [JsonPropertyName("correctIndex")]
    public int CorrectIndex { get; set; }

    [JsonPropertyName("explanation")]
    public string Explanation { get; set; } = string.Empty;
}

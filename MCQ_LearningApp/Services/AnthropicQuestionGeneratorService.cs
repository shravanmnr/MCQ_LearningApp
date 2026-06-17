using System.Text.Json;
using MCQ_LearningApp.Infrastructure.Dto;
using MCQ_LearningApp.Infrastructure.Http;
using MCQ_LearningApp.Models.Domain;
using MCQ_LearningApp.Services.Interfaces;

namespace MCQ_LearningApp.Services;

public class AnthropicQuestionGeneratorService : IQuestionGeneratorService
{
    private readonly AnthropicApiClient _apiClient;
    private readonly ILogger<AnthropicQuestionGeneratorService> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
    };

    private const string SystemPrompt = """
        You are an expert ASP.NET Core trainer creating high-quality MCQ questions for developers
        learning ASP.NET Core MVC. Your questions range from beginner to intermediate difficulty.
        You always return responses as valid JSON only, with no surrounding text.
        """;

    public AnthropicQuestionGeneratorService(AnthropicApiClient apiClient, ILogger<AnthropicQuestionGeneratorService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<QuizQuestion> GenerateAsync(Category category, CancellationToken cancellationToken = default)
    {
        var userMessage = BuildPrompt(category);

        _logger.LogInformation("Generating question for category: {Category}", category.Name);

        var responseText = await _apiClient.GetTextCompletionAsync(SystemPrompt, userMessage, cancellationToken);
        _logger.LogDebug("Raw model response: {Response}", responseText);

        var json = AnthropicApiClient.ExtractJsonFromText(responseText);
        _logger.LogDebug("Extracted JSON: {Json}", json);

        GeneratedQuestionDto dto;
        try
        {
            dto = JsonSerializer.Deserialize<GeneratedQuestionDto>(json, _jsonOptions)
                  ?? throw new InvalidOperationException("Deserialized to null.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parse failed.\nRaw: {Raw}\nExtracted: {Json}", responseText, json);
            throw new InvalidOperationException(
                $"JSON parse error: {ex.Message} | Extracted JSON preview: {json[..Math.Min(400, json.Length)]}", ex);
        }

        ValidateDto(dto);

        return new QuizQuestion
        {
            QuestionText = dto.Question,
            Choices = dto.Choices,
            CorrectIndex = dto.CorrectIndex,
            Explanation = dto.Explanation
        };
    }

    private static string BuildPrompt(Category category) => $$"""
        Generate exactly ONE multiple-choice question about: {{category.Name}}
        Topic context: {{category.Description}}

        Return ONLY valid JSON matching this exact schema — no markdown, no extra text:
        {
          "question": "The full question text here",
          "choices": [
            "First option text",
            "Second option text",
            "Third option text",
            "Fourth option text"
          ],
          "correctIndex": 2,
          "explanation": "A thorough explanation of why the correct answer is right, and why each wrong answer is incorrect. Include short inline code examples where they clarify the concept."
        }

        Rules:
        - correctIndex is 0-based (0, 1, 2, or 3). Vary which index is correct across questions.
        - All four choices must be plausible to a beginner — no obviously silly options.
        - The question must test real conceptual understanding, not just trivia.
        - The explanation must teach the underlying ASP.NET Core concept clearly.
        - Include real code snippets inline (using backticks) where it helps.
        - Never repeat the same question twice — vary phrasing, scenario, and focus.
        """;

    private static void ValidateDto(GeneratedQuestionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Question))
            throw new InvalidOperationException("Generated question text is empty.");
        if (dto.Choices.Count != 4)
            throw new InvalidOperationException($"Expected 4 choices, got {dto.Choices.Count}.");
        if (dto.CorrectIndex < 0 || dto.CorrectIndex > 3)
            throw new InvalidOperationException($"correctIndex {dto.CorrectIndex} is out of range.");
        if (string.IsNullOrWhiteSpace(dto.Explanation))
            throw new InvalidOperationException("Generated explanation is is empty.");
    }
}

using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using MCQ_LearningApp.Infrastructure.Dto;
using MCQ_LearningApp.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace MCQ_LearningApp.Infrastructure.Http;

public class AnthropicApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<AnthropicOptions> _options;
    private readonly ILogger<AnthropicApiClient> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AnthropicApiClient(HttpClient httpClient, IOptions<AnthropicOptions> options, ILogger<AnthropicApiClient> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public async Task<string> GetTextCompletionAsync(string systemPrompt, string userMessage, CancellationToken cancellationToken = default)
    {
        var opts = _options.Value;

        // OpenRouter uses the OpenAI-compatible chat completions format.
        // The system prompt goes in as a "system" role message.
        var requestBody = new AnthropicRequestDto
        {
            Model = opts.Model,
            MaxTokens = opts.MaxTokens,
            Messages = new List<MessageDto>
            {
                new() { Role = "system", Content = systemPrompt },
                new() { Role = "user",   Content = userMessage }
            }
        };

        var requestJson = JsonSerializer.Serialize(requestBody, _jsonOptions);

        // NOTE: no leading slash — HttpClient trims the base path if you use "/path"
        // e.g. BaseAddress="https://openrouter.ai/api/v1/" + "chat/completions"
        //      → https://openrouter.ai/api/v1/chat/completions  ✓
        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
        };

        // OpenRouter auth uses Bearer token, not x-api-key
        request.Headers.Add("Authorization", $"Bearer {opts.ApiKey}");

        // OpenRouter recommends identifying your app; shows up in their dashboard
        request.Headers.Add("X-Title", opts.AppName);

        _logger.LogDebug("Calling OpenRouter with model {Model}", opts.Model);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("OpenRouter API error {StatusCode}: {Error}", response.StatusCode, error);
            throw new HttpRequestException($"OpenRouter returned {response.StatusCode}: {error}");
        }

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var apiResponse = JsonSerializer.Deserialize<AnthropicResponseDto>(responseJson, _jsonOptions);

        var text = apiResponse?.Choices?.FirstOrDefault()?.Message?.Content;
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("OpenRouter returned an empty response.");

        return text;
    }

    public static string ExtractJsonFromText(string text)
    {
        // 1. Prefer an explicit ```json ... ``` fence (language must be "json")
        //    Generic ``` blocks are intentionally excluded — the model may return
        //    ```csharp code before the JSON, which we must not pick up.
        var jsonFence = Regex.Match(text, @"```json\s*([\s\S]*?)\s*```", RegexOptions.IgnoreCase);
        if (jsonFence.Success)
            return jsonFence.Groups[1].Value.Trim();

        // 2. Find the first '{' that is followed by a quoted key — i.e. the start of
        //    a real JSON object.  This skips C# / other code blocks whose { is followed
        //    by whitespace + code, not a string literal.
        var jsonObjectStart = Regex.Match(text, @"\{\s*""");
        if (!jsonObjectStart.Success)
            return text.Trim();

        int start = jsonObjectStart.Index;
        int depth = 0;
        bool inString = false;
        bool escaped = false;

        for (int i = start; i < text.Length; i++)
        {
            char c = text[i];

            if (escaped)               { escaped = false; continue; }
            if (c == '\\' && inString) { escaped = true;  continue; }
            if (c == '"')              { inString = !inString; continue; }
            if (inString)              continue;

            if      (c == '{') depth++;
            else if (c == '}')
            {
                depth--;
                if (depth == 0)
                    return text[start..(i + 1)];
            }
        }

        return text.Trim();
    }
}

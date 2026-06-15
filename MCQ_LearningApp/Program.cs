using MCQ_LearningApp.Infrastructure.Http;
using MCQ_LearningApp.Infrastructure.Options;
using MCQ_LearningApp.Services;
using MCQ_LearningApp.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ── MVC + Antiforgery ───────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── In-memory cache (stores generated questions per session) ────────────────
builder.Services.AddMemoryCache();

// ── Anthropic configuration ─────────────────────────────────────────────────
builder.Services.Configure<AnthropicOptions>(
    builder.Configuration.GetSection(AnthropicOptions.SectionName));

// ── Typed HttpClient for OpenRouter ─────────────────────────────────────────
builder.Services.AddHttpClient<AnthropicApiClient>(client =>
{
    var baseUrl = builder.Configuration[$"{AnthropicOptions.SectionName}:BaseUrl"]
                  ?? "https://openrouter.ai/api/v1";

    // HttpClient requires a trailing slash on BaseAddress so that relative paths
    // like "chat/completions" append correctly instead of replacing the last segment.
    if (!baseUrl.EndsWith('/')) baseUrl += '/';

    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(60);
});

// ── Application services ─────────────────────────────────────────────────────
builder.Services.AddSingleton<ICategoryService, CategoryService>();
builder.Services.AddScoped<IQuestionGeneratorService, AnthropicQuestionGeneratorService>();

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

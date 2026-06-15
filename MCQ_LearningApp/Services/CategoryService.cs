using MCQ_LearningApp.Models.Domain;
using MCQ_LearningApp.Services.Interfaces;

namespace MCQ_LearningApp.Services;

public class CategoryService : ICategoryService
{
    private static readonly IReadOnlyList<Category> _categories =
    [
        new("mvc-architecture",     "MVC Architecture",          "The Model-View-Controller pattern, request lifecycle, and app structure",  "🏗️",  "bg-indigo"),
        new("routing",              "Routing & URLs",             "Conventional routing, attribute routing, route constraints, URL generation", "🛣️",  "bg-blue"),
        new("controllers",          "Controllers & Actions",      "Controller base class, action methods, action results, IActionResult",       "⚙️",  "bg-violet"),
        new("views-razor",          "Views & Razor Syntax",       "Razor directives, Tag Helpers, Partial Views, View Components, Layouts",     "👁️",  "bg-sky"),
        new("model-binding",        "Model Binding",              "Binding request data to parameters, IModelBinder, binding sources",          "🔗",  "bg-cyan"),
        new("validation",           "Validation",                 "Data Annotations, ModelState, FluentValidation, client-side validation",     "✅",  "bg-teal"),
        new("middleware",           "Middleware Pipeline",        "Use(), Run(), Map(), middleware order, short-circuiting, custom middleware",  "🔧",  "bg-emerald"),
        new("dependency-injection", "Dependency Injection",       "IServiceCollection, lifetimes (Singleton/Scoped/Transient), IServiceProvider","💉",  "bg-green"),
        new("entity-framework",     "Entity Framework Core",      "DbContext, DbSet, migrations, LINQ, relationships, change tracking",         "🗄️",  "bg-lime"),
        new("auth-authz",           "Auth & Authorization",       "ASP.NET Identity, JWT, Cookie auth, policies, roles, claims",                "🔐",  "bg-yellow"),
        new("web-api",              "Web API & REST",             "ApiController, HTTP verbs, content negotiation, status codes, Swagger",      "🌐",  "bg-orange"),
        new("configuration",        "Configuration & Settings",   "IConfiguration, appsettings.json, IOptions<T>, environment overrides",       "📁",  "bg-amber"),
        new("logging",              "Logging",                    "ILogger<T>, log levels, structured logging, Serilog, built-in providers",    "📋",  "bg-red"),
        new("filters",              "Filters & Attributes",       "IActionFilter, IResultFilter, IExceptionFilter, global vs. local filters",   "🔍",  "bg-pink"),
        new("state-management",     "State Management",           "Session, Cookies, IMemoryCache, IDistributedCache, TempData",                "💾",  "bg-rose"),
    ];

    public IReadOnlyList<Category> GetAll() => _categories;

    public Category? GetById(string id) =>
        _categories.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
}

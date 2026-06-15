using MCQ_LearningApp.Models.Domain;
using MCQ_LearningApp.Models.ViewModels;
using MCQ_LearningApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MCQ_LearningApp.Controllers;

public class QuizController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly IQuestionGeneratorService _questionGeneratorService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<QuizController> _logger;

    public QuizController(
        ICategoryService categoryService,
        IQuestionGeneratorService questionGeneratorService,
        IMemoryCache cache,
        ILogger<QuizController> logger)
    {
        _categoryService = categoryService;
        _questionGeneratorService = questionGeneratorService;
        _cache = cache;
        _logger = logger;
    }

    // GET /Quiz
    public IActionResult Index()
    {
        var viewModel = new QuizIndexViewModel
        {
            Categories = _categoryService.GetAll()
        };
        return View(viewModel);
    }

    // GET /Quiz/Random  — picks a random category and forwards to Question
    public IActionResult Random()
    {
        var categories = _categoryService.GetAll();
        var picked = categories[System.Random.Shared.Next(categories.Count)];
        return RedirectToAction(nameof(Question), new { categoryId = picked.Id });
    }

    // GET /Quiz/Question/{categoryId}
    public async Task<IActionResult> Question(string categoryId, CancellationToken cancellationToken)
    {
        var category = _categoryService.GetById(categoryId);
        if (category is null)
        {
            TempData["Error"] = $"Topic '{categoryId}' was not found.";
            return RedirectToAction(nameof(Index));
        }

        QuizQuestion question;
        try
        {
            question = await _questionGeneratorService.GenerateAsync(category, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate question for category {CategoryId}", categoryId);

            // In Development, surface the real message so you can diagnose API issues
            var detail = HttpContext.RequestServices
                .GetRequiredService<IWebHostEnvironment>().IsDevelopment()
                ? $" Detail: {ex.Message}"
                : string.Empty;

            TempData["Error"] = $"Could not generate a question right now. Please check your API key or try again.{detail}";
            return RedirectToAction(nameof(Index));
        }

        var questionId = Guid.NewGuid().ToString("N");
        _cache.Set(CacheKey(questionId), question, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
            SlidingExpiration = TimeSpan.FromMinutes(10)
        });

        var viewModel = new QuestionViewModel
        {
            QuestionId = questionId,
            CategoryId = category.Id,
            CategoryName = category.Name,
            CategoryIcon = category.Icon,
            QuestionText = question.QuestionText,
            Choices = question.Choices
        };

        return View(viewModel);
    }

    // POST /Quiz/Submit  (AJAX endpoint — returns JSON)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Submit([FromForm] SubmitAnswerViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { error = "Invalid submission." });

        var question = _cache.Get<QuizQuestion>(CacheKey(model.QuestionId));
        if (question is null)
        {
            return Json(new { error = "This question has expired. Please get a new question." });
        }

        bool isCorrect = model.SelectedIndex == question.CorrectIndex;

        _logger.LogInformation(
            "Quiz answer: category={CategoryId}, correct={IsCorrect}, selected={Selected}, correct={Correct}",
            model.CategoryId, isCorrect, model.SelectedIndex, question.CorrectIndex);

        return Json(new AnswerResultDto
        {
            IsCorrect = isCorrect,
            CorrectIndex = question.CorrectIndex,
            SelectedIndex = model.SelectedIndex,
            Explanation = question.Explanation
        });
    }

    private static string CacheKey(string questionId) => $"quiz_question_{questionId}";
}

using MCQ_LearningApp.Models.Domain;

namespace MCQ_LearningApp.Services.Interfaces;

public interface IQuestionGeneratorService
{
    Task<QuizQuestion> GenerateAsync(Category category, CancellationToken cancellationToken = default);
}

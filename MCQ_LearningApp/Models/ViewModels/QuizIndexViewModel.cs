using MCQ_LearningApp.Models.Domain;

namespace MCQ_LearningApp.Models.ViewModels;

public class QuizIndexViewModel
{
    public IReadOnlyList<Category> Categories { get; set; } = [];
}

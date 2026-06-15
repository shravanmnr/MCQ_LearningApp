namespace MCQ_LearningApp.Models.ViewModels;

public class QuestionViewModel
{
    public string QuestionId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public List<string> Choices { get; set; } = [];
}

namespace MCQ_LearningApp.Models.Domain;

public class QuizQuestion
{
    public string QuestionText { get; set; } = string.Empty;
    public List<string> Choices { get; set; } = new();
    public int CorrectIndex { get; set; }
    public string Explanation { get; set; } = string.Empty;
}

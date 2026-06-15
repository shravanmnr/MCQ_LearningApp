namespace MCQ_LearningApp.Models.ViewModels;

public class AnswerResultDto
{
    public bool IsCorrect { get; set; }
    public int CorrectIndex { get; set; }
    public int SelectedIndex { get; set; }
    public string Explanation { get; set; } = string.Empty;
}

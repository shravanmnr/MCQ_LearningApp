namespace MCQ_LearningApp.Models.ViewModels;

public class SubmitAnswerViewModel
{
    public string QuestionId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public int SelectedIndex { get; set; }
}

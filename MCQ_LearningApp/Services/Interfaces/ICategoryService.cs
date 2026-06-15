using MCQ_LearningApp.Models.Domain;

namespace MCQ_LearningApp.Services.Interfaces;

public interface ICategoryService
{
    IReadOnlyList<Category> GetAll();
    Category? GetById(string id);
}

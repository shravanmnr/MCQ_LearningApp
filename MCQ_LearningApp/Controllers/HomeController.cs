using MCQ_LearningApp.Models;
using MCQ_LearningApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MCQ_LearningApp.Controllers;

public class HomeController : Controller
{
    private readonly ICategoryService _categoryService;

    public HomeController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public IActionResult Index()
    {
        var categories = _categoryService.GetAll();
        return View(categories);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    [Route("DefaultLayer")]
    public IActionResult DefaultLayer()
	{
        if(Request.Query.ContainsKey("IsAuthenticated"))
		{
			return Content("Default layer is not implemented yet.");
		}
		return Content(" ");

	}
}

using Microsoft.AspNetCore.Mvc;
using StudentsWeb.Filters;
using StudentsWeb.Models;
using StudentsWeb.Services;

namespace StudentsWeb.Controllers;

[RequireLogin]
public class StudentsController : Controller
{
    private readonly StudentsApiClient _apiClient;

    public StudentsController(StudentsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var students = await _apiClient.GetStudentsAsync();
            return View(students);
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Could not load students. Please sign in again.";
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var student = await _apiClient.GetStudentAsync(id);
            if (student is null)
            {
                return NotFound();
            }

            return View(student);
        }
        catch (HttpRequestException)
        {
            return RedirectToAction("Login", "Account");
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new StudentFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StudentFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _apiClient.CreateStudentAsync(model);
            TempData["Success"] = "Student added successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException ex)
        {
            model.ErrorMessage = ex.Message;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var student = await _apiClient.GetStudentAsync(id);
        if (student is null)
        {
            return NotFound();
        }

        return View(new StudentFormViewModel
        {
            Id = student.Id,
            Name = student.Name,
            Email = student.Email,
            Phone = student.Phone
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, StudentFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _apiClient.UpdateStudentAsync(id, model);
            TempData["Success"] = "Student updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException ex)
        {
            model.ErrorMessage = ex.Message;
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _apiClient.DeleteStudentAsync(id);
            TempData["Success"] = "Student deleted successfully.";
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Could not delete student.";
        }

        return RedirectToAction(nameof(Index));
    }
}

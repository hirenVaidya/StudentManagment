using Microsoft.AspNetCore.Mvc;
using StudentsWeb.Models;
using StudentsWeb.Services;

namespace StudentsWeb.Controllers;

public class AccountController : Controller
{
    private readonly StudentsApiClient _apiClient;

    public AccountController(StudentsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKeys.JwtToken)))
        {
            return RedirectToAction("Index", "Students");
        }

        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, data, error) = await _apiClient.LoginAsync(model.Email, model.Password);
        if (!success || data is null)
        {
            model.ErrorMessage = error;
            return View(model);
        }

        HttpContext.Session.SetString(SessionKeys.JwtToken, data.Token);
        HttpContext.Session.SetString(SessionKeys.UserName, data.Name);
        HttpContext.Session.SetString(SessionKeys.UserEmail, data.Email);

        return RedirectToAction("Index", "Students");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
}

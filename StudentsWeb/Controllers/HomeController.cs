using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StudentsWeb.Models;
using StudentsWeb.Services;

namespace StudentsWeb.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKeys.JwtToken)))
        {
            return RedirectToAction("Index", "Students");
        }

        return RedirectToAction("Login", "Account");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

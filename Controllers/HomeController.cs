using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace InventoryManagement.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly IEmailSender _emailSender;
    public HomeController(ILogger<HomeController> logger, IEmailSender emailSender)
    {
        _logger = logger;
        _emailSender = emailSender;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
    
    [HttpGet]
    public async Task<IActionResult> TestEmail()
    {
        await _emailSender.SendEmailAsync(
            "youremail@gmail.com",
            "Email Test from ASP.NET App",
            "<strong>This is a test email sent from your Inventory App!</strong>"
        );

        return Content("Email sent successfully (check your inbox).");
    }

}

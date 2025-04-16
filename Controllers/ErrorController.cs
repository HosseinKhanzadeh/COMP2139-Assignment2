using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using InventoryManagement.Models;

namespace InventoryManagement.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult NotFound()
        {
            return View();
        }

        public IActionResult ServerError()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
} 
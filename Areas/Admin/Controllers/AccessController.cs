using Microsoft.AspNetCore.Mvc;

namespace TourismManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccessController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string adminCode)
        {
            const string correctCode = "Atchyuth@j110";

            if (adminCode == correctCode)
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

            }

            ViewBag.ErrorMessage = "Incorrect admin code.";
            return View();
        }
    }
}

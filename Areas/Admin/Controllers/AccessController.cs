using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace TourismManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccessController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccessController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string adminCode)
        {
            const string correctCode = "Atchyuth@j110";

            if (adminCode == correctCode)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    // Ensure the Admin role exists
                    if (!await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        await _userManager.AddToRoleAsync(user, "Admin");

                        // Refresh sign-in so new role claim is in the cookie
                        await _signInManager.RefreshSignInAsync(user);
                    }
                }

                return RedirectToAction("Index", "Packages", new { area = "Admin" });
            }

            ViewBag.ErrorMessage = "Incorrect admin code.";
            return View();
        }
    }
}

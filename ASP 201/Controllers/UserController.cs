using ASP_201.Models.User;
using Microsoft.AspNetCore.Mvc;

namespace ASP_201.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Registration()
        {
            return View();
        }
        public IActionResult Register(RegistrationModel model)
        {
            ViewData["registrationModel"] = model;
            // спосіб перейти на View з іншою назвою, ніж метод
            return View("Registration");
        }
    }
}

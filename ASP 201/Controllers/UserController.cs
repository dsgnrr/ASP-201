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
        public IActionResult Register(RegistrationModel registrationModel)
        {
            bool isModelValid = true;
            RegisterValidationModel registerValidation = new();
            if(String.IsNullOrEmpty(registrationModel.Login))
            {
                registerValidation.LoginMessage = "Логін не може бути порожнім";
                isModelValid = false;
            }

            ViewData["registrationModel"] = registerValidation;
            // спосіб перейти на View з іншою назвою, ніж метод
            return View("Registration");
        }
    }
}

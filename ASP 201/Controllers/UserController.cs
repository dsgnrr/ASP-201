using ASP_201.Data;
using ASP_201.Models.User;
using ASP_201.Services.Hash;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace ASP_201.Controllers
{
    public class UserController : Controller
    {
        private ILogger<UserController> _logger;
        private readonly IHashService _hashService;
        private readonly DataContext dataContext;

        public UserController(IHashService hashService,
                              ILogger<UserController> logger,
                              DataContext dataContext)
        {
            _hashService = hashService;
            _logger = logger;
            this.dataContext = dataContext;
        }

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
            byte minPasswordLength = 3;
            RegisterValidationModel registerValidation = new();
            #region Login Validation
            if (String.IsNullOrEmpty(registrationModel.Login))
            {
                registerValidation.LoginMessage = "Логін не може бути порожнім";
                isModelValid = false;
            }
            #endregion

            #region Password / Repeat Validation
            if (String.IsNullOrEmpty(registrationModel.Password))
            {
                registerValidation.PasswordMessage = "Пароль не може бути порожнім";
                registerValidation.RepeatPasswordMessage = "";
                isModelValid = false;
            }
            else if (registrationModel.Password.Length < minPasswordLength)
            {
                registerValidation.PasswordMessage =
                    $"Пароль закороткий. Щонайменше символів — {minPasswordLength}";
                registerValidation.RepeatPasswordMessage = "";
                isModelValid = false;
            }
            else if (!registrationModel.Password.Equals(registrationModel.RepeatPassword))
            {
                registerValidation.PasswordMessage =
                    registerValidation.RepeatPasswordMessage = "Паролі не збігаються";
                isModelValid = false;
            }
            #endregion

            #region Email Validation
            if (String.IsNullOrEmpty(registrationModel.Email))
            {
                registerValidation.EmailMessage = "E-mail не може бути порожнім";
                isModelValid = false;
            }
            else
            {
                String emailRegex = @"^[\w.%+-]+@[\w.-]+\.[a-zA-Z]{2,}$";
                if(!Regex.IsMatch(registrationModel.Email,emailRegex))
                {
                    registerValidation.EmailMessage = "E-mail не відповідає шаблону";
                    isModelValid = false;
                }
            }
            #endregion

            #region Real Name Validation
            if (String.IsNullOrEmpty(registrationModel.RealName))
            {
                registerValidation.RealNameMessage = "Ім'я не може бути порожнім";
                isModelValid = false;
            }
            else
            {
                String nameRegex = @"^.+$";
                if (!Regex.IsMatch(registrationModel.RealName, nameRegex))
                {
                    registerValidation.RealNameMessage = "Ім'я не відповідає шаблону";
                    isModelValid = false;
                }
            }
            #endregion

            #region IsAgree Validation
            if(registrationModel.IsAgree==false)
            {
                registerValidation.IsAgreeMessage = "Для реєстрації слід прийняти правила сайту";
                isModelValid = false;
            }
            #endregion

            #region Avatar
            // будемо вважати аватар необов'язковим, обробляємо лише якщо переданий
            if(registrationModel.Avatar is not null) //є файл
            {
                if (registrationModel.Avatar.Length > 1024)
                {
                    // Генеруємо для файла нове ім'я, але зберігаємо розширення
                    String ext = Path.GetExtension(registrationModel.Avatar.FileName);
                    // TODO: перевірити розширення на перелік дозволених
                    String savedName = _hashService.Hash(
                    registrationModel.Avatar.FileName + DateTime.Now)[..16]
                        +ext;
                    String path = "wwwroot/avatars/" + savedName;
                    
                    while(System.IO.File.Exists(path))
                    {
                        savedName = _hashService.Hash(
                            registrationModel.Avatar.FileName + DateTime.Now)[..16]
                            + ext;
                        path = "wwwroot/avatars/" + savedName;
                    }
                    using FileStream fs = new(path, FileMode.Create);
                    registrationModel.Avatar.CopyTo(fs);
                    ViewData["savedName"] = savedName;
                }
                else
                {
                    registerValidation.AvatarMessage = "Розмір аватарки має бути більше 1 кБ";
                    isModelValid= false;
                }
            }

            #endregion

            // якщо всі перевірки пройдені, то переходимо на нову сторінку з вітаннями
            if (isModelValid)
            {
                return View(registrationModel);
            }
            else // не всі дані валідні — повертаємо на форму реєстрації
            {
                // передаємо дані щодо перевірок
                ViewData["registrationModel"] = registerValidation;
                // спосіб перейти на View з іншою назвою, ніж метод
                return View("Registration");
            }

           
        }
    }
}

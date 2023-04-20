﻿using ASP_201.Data;
using ASP_201.Data.Entity;
using ASP_201.Models.User;
using ASP_201.Services.Hash;
using ASP_201.Services.Kdf;
using ASP_201.Services.Random;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ASP_201.Controllers
{
    public class UserController : Controller
    {
        private ILogger<UserController> _logger;
        private readonly IHashService _hashService;
        private readonly DataContext dataContext;
        private readonly IRandomService randomService;
        private readonly IKdfService kdfService;
        public UserController(IHashService hashService,
                              ILogger<UserController> logger,
                              DataContext dataContext,
                              IRandomService randomService,
                              IKdfService kdfService)
        {
            _hashService = hashService;
            _logger = logger;
            this.dataContext = dataContext;
            this.randomService = randomService;
            this.kdfService = kdfService;
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
            if(dataContext.Users.Count(u=>u.Login==registrationModel.Login)>0)
            {
                registerValidation.LoginMessage = "Логін вже використовується";
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
            String savedName = null;
            if (registrationModel.Avatar is not null) //є файл
            {
                if (registrationModel.Avatar.Length > 1024)
                {
                    savedName = randomService.AvatarPhotoName(registrationModel.Avatar.FileName);
                    String path = "wwwroot/avatars/" + savedName;
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
                String salt = randomService.RandomString(16);
                User user = new()
                {
                    Id = Guid.NewGuid(),
                    Login = registrationModel.Login,
                    RealName = registrationModel.RealName,
                    Email = registrationModel.Email,
                    EmailCode = randomService.ConfirmCode(6),
                    PasswordSalt = salt,
                    PasswordHash = kdfService.GetDerivedKey(registrationModel.Password, salt),
                    Avatar = savedName,
                    RegisterDt = DateTime.Now,
                    LastEnterDt = null
                };
                dataContext.Users.Add(user);
                dataContext.SaveChangesAsync();

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
        
        [HttpPost] // метод доступний тільки для POST запитів
        public String AuthUser()
        {
            // альтернативний (до моделей) спосіб отримання параметрів запиту
            StringValues loginValues = Request.Form["user-login"];
            StringValues paswwordValues = Request.Form["user-password"];
            // колекція loginValues формується при будь-якому ключі, але для
            // неправильних (відсутніх) ключів вона порожня
            if (loginValues.Count == 0) 
            {
                // немає логіну у складі полів
                return "Missed required parameter: user-login";
            }
           
            if (paswwordValues.Count == 0)
            {
                // немає логіну у складі полів
                return "Missed required parameter: user-password";
            }

            String login = loginValues[0] ?? "";
            String password = paswwordValues[0] ?? "";
            
            // шукаємо користувача за логіном
            User? user = dataContext.Users
                .Where(u => u.Login == login)
                .FirstOrDefault();
            if(user is not null)
            {
                // якщо знайшли - перевіряємо пароль (derived key)
                if (user.PasswordHash == kdfService
                    .GetDerivedKey(password, user.PasswordSalt)) 
                {
                    //дані перевірені - користувач автентифікований - зберігаємо у сесії
                    HttpContext.Session.SetString("authUserId", user.Id.ToString());
                    return "OK";
                }
            }

            return "Авторизацію відхилено";
        }

        public RedirectToActionResult Logout()
        {
            HttpContext.Session.Remove("authUserId");
            return RedirectToAction("Index", "Home");
            /*  Redirect та інші питання з перенаправлення
             *  Browser         Server
             *  GET /home -----> (routing)->Home::Index()->View()
             *            <----- 200 OK <!doctype html>...
             *            
             *  <a Logout> -----> User::Logout()->Redirect(...)
             *  follow     <----- 302 (Redirect) Location: /home
             *  GET /home  -----> (routing)->Home::Index()->View()
             *    page     <----- 200 OK <!doctype html>... 
             *    
             *  301 - Permanent Redirect - перенесено на постійній основі,
             *  як правило, сайт змінив URL
             *  Довільний редірект слудіється GET запитом, якщо потрібно
             *  зберігти початковий метод, то вживається
             *  Redirect...PreserveMethod
             *  
             *  30х Redirect називають зовнішними, тому що інформація
             *  доходить до браузера і змінюється URL в адресному рядку
             *  http://.../addr1 ---> 302 Location /addr2
             *  http://.../addr1 ---> 200 html
             *  
             *                               addr1.asp
             * http://.../addr1(if...)   \   addr1.asp
             *                            \  addr1.asp
             *                      forward - внутрінє перенаправлення
             *  (у браузері /addr1, але фактично відображено addr3.asp)     
             */
        }
        public IActionResult Profile([FromRoute]String id)
        {
            // Задача: реалізувати можливість розрізнення
            // власного та чужого профілів
            // _logger.LogInformation(id);
            User? user = dataContext.Users
                .FirstOrDefault(u => u.Login == id);
            if (user is not null)
            {
                Models.User.ProfileModel model = new(user);
                // дістаємо відомості про автентифікацію
                if(HttpContext.User.Identity is not null
                    && HttpContext.User.Identity.IsAuthenticated)
                {
                    String userLogin =
                        HttpContext.User.Claims
                        .First(c => c.Type == ClaimTypes.NameIdentifier)
                        .Value;
                    if(userLogin==user.Login)
                    {
                        model.IsPersonal = true;
                    }
                }
                return View(model);
            }
            else
            {
                return NotFound();
            }
            /* Особиста сторінка / Профіль
             * 1. Чи буде ця сторінка доступна іншим користувачам?
             *  Так, користувачі можуть переглядати профіль інших користувачів
             *  але тільки ті дані, що дозволив власник.
             * 2. Як має формуватись адреса /User/Profile/????
             *  а) Id
             *  б) логін
             *  Обираємо логін, в силу зручності поширення  посилання на власний
             *  профіль
             *  !! необхідно забезпечити унікальність логіну
             */
        }
    }
}

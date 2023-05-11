using ASP_201.Data;
using ASP_201.Data.Entity;
using ASP_201.Models;
using ASP_201.Models.User;
using ASP_201.Services.Email;
using ASP_201.Services.Hash;
using ASP_201.Services.Kdf;
using ASP_201.Services.Random;
using ASP_201.Services.Validation;
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
        private readonly IValidationService validationService;
        private readonly IEmailService emailService;
        public UserController(IHashService hashService,
                              ILogger<UserController> logger,
                              DataContext dataContext,
                              IRandomService randomService,
                              IKdfService kdfService,
                              IValidationService validationService,
                              IEmailService emailService)
        {
            _hashService = hashService;
            _logger = logger;
            this.dataContext = dataContext;
            this.randomService = randomService;
            this.kdfService = kdfService;
            this.validationService = validationService;
            this.emailService = emailService;
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
            if (!validationService.Validate(registrationModel.Email,ValidationTerms.NotEmpty))
            {
                registerValidation.EmailMessage = "E-mail не може бути порожнім";
                isModelValid = false;
            }
            else if(!validationService.Validate(registrationModel.Email,ValidationTerms.Email))
            {
                registerValidation.EmailMessage = "E-mail не відповідає шаблону";
                isModelValid = false;
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
                    String path = "wwwroot/avatars/";
                    savedName = randomService.GeneratePhotoName(registrationModel.Avatar.FileName,path);
                    String resultPath = "wwwroot/avatars/" + savedName;
                    using FileStream fs = new(resultPath, FileMode.Create);
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
                String confirmEmailCode = randomService.ConfirmCode(6);
               
                User user = new()
                {
                    Id = Guid.NewGuid(),
                    Login = registrationModel.Login,
                    RealName = registrationModel.RealName,
                    Email = registrationModel.Email,
                    EmailCode = confirmEmailCode,
                    PasswordSalt = salt,
                    PasswordHash = kdfService.GetDerivedKey(registrationModel.Password, salt),
                    Avatar = savedName,
                    RegisterDt = DateTime.Now,
                    LastEnterDt = null
                };
                dataContext.Users.Add(user);

                // Якщо дані у БД додані, надсилаємо код підтвердження
                // генеруємо токен автоматичного підтвердження
                var emailConfirmToken = _GenerateEmailConfirmToken(user);

                // Надсилаємо листа з токеном
                SendConfirmEmail(user, emailConfirmToken);
                
                dataContext.SaveChanges();
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
        // /User/Profile/Admin : User-controlller, Profile-action, Admin-id
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

        [HttpPut] // метод доступний тільки для PUT запитів
        public IActionResult Update([FromBody]UpdateRequestModel model)
        {
            UpdateResponseModel responseModel = new();
            try
            {
                if (model is null) throw new Exception("No or empty data");
                if (HttpContext.User.Identity?.IsAuthenticated == false)
                {
                    throw new Exception("UnAuthenticated");
                }
                User? user =
                    dataContext.Users.Find(
                        Guid.Parse(
                            HttpContext.User.Claims
                            .First(c => c.Type == ClaimTypes.Sid)
                            .Value
                            ));
                if (user is null) throw new Exception("UnAuthorized");
                switch (model.Field)
                {
                    case "realname":
                        if (validationService.Validate(
                            model.Value,
                            ValidationTerms.RealName))
                        {
                            user.RealName = model.Value;
                            dataContext.SaveChanges();
                        }
                        else throw new Exception(
                            $"Validation error: field'{model.Field}' with value '{model.Value}'");
                        break;
                    case "email":
                        if (validationService.Validate(model.Value, ValidationTerms.Email))
                        {
                            user.Email = model.Value;
                            dataContext.SaveChanges();
                            ResendConfirmEmail();
                        }
                        else throw new Exception(
                            $"Validation error: field '{model.Field}' with value '{model.Value}'");
                        break;
                    default:
                        throw new Exception($"Invalid 'Field' attribute: '{model.Field}'");
                }
                responseModel.Status = "OK";
                responseModel.Data = $"Field '{model.Field}' updated by value '{model.Value}'";
                
            }
            catch(Exception ex)
            {
                responseModel.Status = "Error";
                responseModel.Data = ex.Message;
            }
            return Json(new { responseModel });
            /* Метод для оновлення даних про користувача
             * Приймає асинхронні запити з JSON даними, повертає JSON
             * із результатом роботи.
             * Приймає дані = описуємо модель цих даних
             * Повертає дані = описуємо модель
             */
        }

        [HttpPost]
        public JsonResult ConfirmEmail([FromBody] string emailCode)
        {
            StatusDataModel model = new();

            if(String.IsNullOrEmpty(emailCode))
            {
                model.Status ="406";
                model.Data = "Empty code not acceptable";
            }    
            else if(HttpContext.User.Identity?.IsAuthenticated == false)
            {
                model.Status = "401";
                model.Data = "Unauthenticated";
            }
            else
            {
                User? user =
                   dataContext.Users.Find(
                       Guid.Parse(
                           HttpContext.User.Claims
                           .First(c => c.Type == ClaimTypes.Sid)
                           .Value
                           ));
                if(user is null)
                {
                    model.Status = "403";
                    model.Data = "Forbidden (UnAthorized)";
                }
                else if (user.EmailCode is null)
                {
                    model.Status = "208";
                    model.Data = "Already confirmed";
                }
                else if(user.EmailCode!=emailCode)
                {
                    model.Status = "406";
                    model.Data = "Code not Accepted";
                }
                else
                {
                    user.EmailCode = null;
                    dataContext.SaveChanges();
                    model.Status = "200";
                    model.Data = "OK";
                }
            }
            return Json(model);
        }
        [HttpGet]
        public ViewResult ConfirmToken([FromQuery] String token)
        {
            try
            {
                // шукаєо токе за отрианим Id
                var confirmToken = dataContext.EmailConfirmTokens
                    .Find(Guid.Parse(token))
                    ?? throw new Exception();
                // шукаєо користувача за UserId
                var user = dataContext.Users
                    .Find(confirmToken.UserId)
                    ?? throw new Exception();
                if (user.Email != confirmToken.UserEmail)
                    throw new Exception();
                // оновлюємо дані
                user.EmailCode = null;  // пошта підтверджена
                confirmToken.Used += 1; // ведемо рахунок використання токена
                dataContext.SaveChanges();
                ViewData["result"] = "Вітаємо, пошта успішно підтверджена";
            }

            catch
            {
                ViewData["result"] = "Перевірка не пройдена, не змінюйте посилання з листа";
            }
            
            return View();
        }
        private EmailConfirmToken _GenerateEmailConfirmToken(Data.Entity.User user)
        {
            Data.Entity.EmailConfirmToken emailConfirmToken = new()
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                UserEmail = user.Email,
                Moment = DateTime.Now,
                Used = 0
            };
            dataContext.EmailConfirmTokens.Add(emailConfirmToken);
            return emailConfirmToken;
        }

        [HttpPatch]
        public String ResendConfirmEmail()
        {
            if(HttpContext.User.Identity?.IsAuthenticated==false)
            {
                return "Unauthenticated";
            }
            try
            {
                User? user = dataContext.Users
                    .Find(
                    Guid.Parse(
                        HttpContext.User.Claims
                        .First(c => c.Type == ClaimTypes.Sid)
                        .Value
                        )) ?? throw new Exception();
                // формуємо новий код підтвердження пошти
                user.EmailCode = randomService.ConfirmCode(6);
                // генеруєио токен автоматичного підтвердження
                var emailConfirmToken = _GenerateEmailConfirmToken(user);

                // зберігаємо новий код і токен
                dataContext.SaveChanges();

                // надсилаємо листа
                if (SendConfirmEmail(user, emailConfirmToken))
                    return "OK";
                else
                    return "Send error";
            }
            catch
            {
                return "Unauthenticated";
            }
        }
        private bool SendConfirmEmail(Data.Entity.User user,
                                      Data.Entity.EmailConfirmToken emailConfirmToken)
        {
            //  формуємо посилання: схема://домен/User/ConfirmToken?token=...
            //  схема - http або https домен(хост) - localhost:7572 
            String confirmLink = $"{HttpContext.Request.Scheme}" +
                $"://{HttpContext.Request.Host.Value}" +
                $"/User/ConfirmToken?token={emailConfirmToken.Id}";

            return emailService.Send(
                "confirm_email",
                new Models.Email.ConfirmEmailModel
                {
                    Email = user.Email,
                    RealName = user.RealName,
                    EmailCode = user.EmailCode!,
                    ConfirmLink = confirmLink
                });
        }
    }
}

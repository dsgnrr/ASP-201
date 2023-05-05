using ASP_201.Data;
using ASP_201.Models.Forum;
using ASP_201.Services.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;
using System.Security.Claims;

namespace ASP_201.Controllers
{
    public class ForumController:Controller
    {
        private readonly DataContext dataContext;
        private readonly ILogger<ForumController> logger;
        private readonly IValidationService validationService;

        public ForumController(DataContext dataContext, ILogger<ForumController> logger, IValidationService validationService)
        {
            this.dataContext = dataContext;
            this.logger = logger;
            this.validationService = validationService;
        }
        private int _counter = 0;
        private int Counter { get => _counter++; set => _counter = value; }
        public IActionResult Index()
        {
            ForumIndexModel model = new()
            {
                UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
                Sections = dataContext
                    .Sections
                    .Include(s => s.Author) // включити навігаційну валстивість Author
                    .Where(s => s.DeletedDt == null)
                    .OrderByDescending(s => s.CreatedDt)
                    .AsEnumerable() // перетворить IQueriable в IEnumerable
                    .Select(s => new ForumSectionViewModel()
                    {
                        Title = s.Title,
                        Description = s.Description,
                        LogoUrl = $"/img/logos/section{Counter}.png",
                        CreatedDtString = DateTime.Today == s.CreatedDt.Date
                            ? "Сьогодні "
                            : s.CreatedDt.ToString("dd.MM.yyyy HH:mm"),
                        UrlIdString = s.Id.ToString(),
                        AuthorName = s.Author.IsRealNamePublic == true
                        ? s.Author.RealName
                        : s.Author.Login,
                        AuthorAvatarUrl = s.Author.Avatar == null
                            ? "/avatars/no-avatar.png"
                            : $"/avatars/{s.Author.Avatar}"
                    })
                    .ToList()
            };
            if (HttpContext.Session.GetString("CreateSectionMesssage") is String message)
            {
                model.CreateMessage = message;
                model.IsMessagePositive = HttpContext.Session.GetInt32("isMessagePositive") == 1;
                if(model.IsMessagePositive==false)
                {
                    model.formModel = new()
                    {
                        Title = HttpContext.Session.GetString("SavedTitle")!,
                        Description = HttpContext.Session.GetString("SavedDescription")!
                    };
                    HttpContext.Session.Remove("SavedTitle");
                    HttpContext.Session.Remove("SavedDescription");
                }
                HttpContext.Session.Remove("CreateSectionMesssage");
                HttpContext.Session.Remove("isMessagePositive");
            }
            return View(model);
        }

        public ViewResult Sections([FromRoute] String id)
        {
            ForumSectionsModel model = new()
            {
                UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
                SectionId = id,
                Themes = dataContext
                    .Themes
                    .Where(t => t.DeletedDt == null&&t.SectionId==Guid.Parse(id))
                    .Select(t => new ForumThemeViewModel()
                    {
                        Title = t.Title,
                        Description = t.Description,
                        CreatedDtString = DateTime.Today == t.CreatedDt.Date
                            ? "Сьогодні "
                            : t.CreatedDt.ToString("dd.MM.yyyy HH:mm"),
                        UrlIdString = t.Id.ToString(),
                        SectionId = t.SectionId.ToString()
                    })
                    .ToList()
            };

            if (HttpContext.Session.GetString("CreateThemesMesssage") is String message)
            {
                model.CreateMessage = message;
                if (HttpContext.Session.GetInt32("isMessagePositive") == 1)
                    model.IsMessagePositive = true;
                else
                    model.IsMessagePositive = false;
                //model.IsMessagePositive = HttpContext.Session.GetInt32("IsMessagePositive") == 1;
                if (model.IsMessagePositive == false)
                {
                    model.formModel = new()
                    {
                        Title = HttpContext.Session.GetString("SavedTitle")!,
                        Description = HttpContext.Session.GetString("SavedDescription")!
                    };
                    HttpContext.Session.Remove("SavedTitle");
                    HttpContext.Session.Remove("SavedDescription");
                }
                HttpContext.Session.Remove("CreateThemesMesssage");
                HttpContext.Session.Remove("isMessagePositive");
            }

            return View(model);
        }

        [HttpPost]
        public RedirectToActionResult CreateSection(ForumSectionFormModel forumModel)
        {
            logger.LogInformation("Title: {t}, Description {d}",
                forumModel.Title, forumModel.Description);
            if (!validationService.Validate(forumModel.Title, ValidationTerms.NotEmpty))
            {
                HttpContext.Session.SetString("CreateSectionMesssage",
                    "Назва не може бути порожньою");
                HttpContext.Session.SetInt32("isMessagePositive", 0);
                HttpContext.Session.SetString("SavedTitle", forumModel.Title ?? String.Empty);
                HttpContext.Session.SetString("SavedDescription", forumModel.Description ?? String.Empty);
            }
            else if(!validationService.Validate(forumModel.Description, ValidationTerms.NotEmpty))
            {
                HttpContext.Session.SetString("CreateSectionMesssage",
                    "Опис не може бути порожнім");
                HttpContext.Session.SetInt32("isMessagePositive", 0);
                HttpContext.Session.SetString("SavedTitle", forumModel.Title ?? String.Empty);
                HttpContext.Session.SetString("SavedDescription", forumModel.Description ?? String.Empty);
            }
            else
            {
                Guid userId;
                try
                {
                    userId=Guid.Parse(
                        HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value
                        );
                    dataContext.Sections.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        Title = forumModel.Title,
                        Description = forumModel.Description,
                        CreatedDt = DateTime.Now,
                        AuthorId = userId
                    });
                    dataContext.SaveChanges();
                    HttpContext.Session.SetString("CreateSectionMesssage",
                        "Додано успішно");
                    HttpContext.Session.SetInt32("isMessagePositive", 1);

                }
                catch
                {
                    HttpContext.Session.SetString("CreateSectionMesssage",
                     "Відмовлено в авторизації");
                    HttpContext.Session.SetInt32("isMessagePositive", 0);
                    HttpContext.Session.SetString("SavedTitle", forumModel.Title ?? String.Empty);
                    HttpContext.Session.SetString("SavedDescription", forumModel.Description ?? String.Empty);
                }
               
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public RedirectToActionResult CreateTheme(ForumThemeFormModel formModel)
        {
            logger.LogInformation("Title: {t}, Description {d}",
                formModel.Title, formModel.Description);
            if (!validationService.Validate(formModel.Title, ValidationTerms.NotEmpty))
            {
                HttpContext.Session.SetString("CreateThemesMesssage",
                    "Назва не може бути порожньою");
                HttpContext.Session.SetInt32("isMessagePositive", 0);
                HttpContext.Session.SetString("SavedTitle", formModel.Title ?? String.Empty);
                HttpContext.Session.SetString("SavedDescription", formModel.Description ?? String.Empty);
            }
            else if (!validationService.Validate(formModel.Description, ValidationTerms.NotEmpty))
            {
                HttpContext.Session.SetString("CreateThemesMesssage",
                    "Опис не може бути порожнім");
                HttpContext.Session.SetInt32("isMessagePositive", 0);
                HttpContext.Session.SetString("SavedTitle", formModel.Title ?? String.Empty);
                HttpContext.Session.SetString("SavedDescription", formModel.Description ?? String.Empty);
            }
            else
            {
                Guid userId;
                try
                {
                    userId = Guid.Parse(
                        HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value
                        );
                    dataContext.Themes.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        Title = formModel.Title,
                        Description = formModel.Description,
                        CreatedDt = DateTime.Now,
                        AuthorId = userId,
                        SectionId = Guid.Parse(formModel.SectionId)
                    });
                    dataContext.SaveChanges();
                    HttpContext.Session.SetString("CreateThemesMesssage",
                        "Додано успішно");
                    HttpContext.Session.SetInt32("isMessagePositive", 1);

                }
                catch
                {
                    HttpContext.Session.SetString("CreateThemesMesssage",
                     "Відмовлено в авторизації");
                    HttpContext.Session.SetInt32("isMessagePositive", 0);
                    HttpContext.Session.SetString("SavedTitle", formModel.Title ?? String.Empty);
                    HttpContext.Session.SetString("SavedDescription", formModel.Description ?? String.Empty);
                }

            }
            return RedirectToAction(nameof(Sections), new { id = formModel.SectionId });
        }
    }
}

using ASP_201.Data;
using ASP_201.Models.Forum;
using ASP_201.Services.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
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

        public IActionResult Index()
        {
            ForumIndexModel model = new()
            {
                UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
                Sections = dataContext
                    .Sections
                    .Where(s => s.DeletedDt == null)
                    .OrderByDescending(s => s.CreatedDt)
                    .ToList()
            };
            if (HttpContext.Session.GetString("CreateSectionMesssage") is String message)
            {
                HttpContext.Session.Remove("CreateSectionMesssage");
                model.CreateMessage = message;
                model.IsMessagePositive = HttpContext.Session.GetInt32("IsMessagePositive") != 0;
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

        [HttpPost]
        public RedirectToActionResult CreateSection(ForumSectionModel forumModel)
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
    }
}

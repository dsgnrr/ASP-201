using ASP_201.Data;
using ASP_201.Models.Forum;
using ASP_201.Services.Random;
using ASP_201.Services.Transliterate;
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
        private readonly ITransliterationService transliterateService;
        private readonly IRandomService randomService;

        public ForumController(DataContext dataContext, 
               ILogger<ForumController> logger, 
               IValidationService validationService, 
               ITransliterationService transliterateService,
               IRandomService randomService)
        {
            this.dataContext = dataContext;
            this.logger = logger;
            this.validationService = validationService;
            this.transliterateService = transliterateService;
            this.randomService = randomService;
        }
        private int _counter = 0;
        private int Counter {
            get {
                if ((_counter++) == 10) 
                {
                    _counter = 0;
                    return _counter; 
                }
               return _counter++; 
            } 
            set => _counter = value; }
        public IActionResult Index()
        {
            String? userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            ForumIndexModel model = new()
            {
                UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
                Sections = dataContext
                    .Sections
                    .Include(s => s.Author) // включити навігаційну валстивість Author
                    .Include(s => s.RateList)
                    .Where(s => s.DeletedDt == null)
                    .OrderByDescending(s => s.CreatedDt)
                    .AsEnumerable() // перетворить IQueriable в IEnumerable
                    .Select(s => new ForumSectionViewModel()
                    {
                        Title = s.Title,
                        Description = s.Description,
                        LogoUrl = String.IsNullOrEmpty(s.SectionLogo)
                            ? $"/img/logos/section{Counter}.png"
                            : $"/img/logos/" + s.SectionLogo,
                        CreatedDtString = DateTime.Today == s.CreatedDt.Date
                            ? "Сьогодні "
                            : s.CreatedDt.ToString("dd.MM.yyyy HH:mm"),
                        UrlIdString = s.UrlId ?? s.Id.ToString(),
                        IdString = s.Id.ToString(),
                        AuthorName = s.Author.IsRealNamePublic == true
                        ? s.Author.RealName
                        : s.Author.Login,
                        AuthorAvatarUrl = s.Author.Avatar == null
                            ? "/avatars/no-avatar.png"
                            : $"/avatars/{s.Author.Avatar}",
                        LikesCount = s.RateList.Count(r => r.Rating > 0),
                        DislikesCount = s.RateList.Count(r => r.Rating < 0),
                        GivenRating = userId == null ? null
                        : s.RateList.FirstOrDefault(r => r.UserId == Guid.Parse(userId))?.Rating
                    })
                    .ToList()
            };
            if (HttpContext.Session.GetString("CreateSectionMesssage") is String message)
            {
                model.CreateMessage = message;
                model.IsMessagePositive = HttpContext.Session.GetInt32("isMessagePositive") == 1;
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
                HttpContext.Session.Remove("CreateSectionMesssage");
                HttpContext.Session.Remove("isMessagePositive");
            }
            return View(model);
            //return View();
        }

        public IActionResult Topics([FromRoute] String id)
        {
            // ForumTopicsModel ForumPostFormModel ForumPostViewModel
            Guid topicId;
            try
            {
                topicId = Guid.Parse(id);
            }
            catch
            {
                topicId = Guid.Empty;
            }
            var topic = dataContext.Topics.Find(topicId);
            if(topic==null)
            {
                return NotFound();
            }


            ForumTopicsModel model = new()
            {
                UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
                Title = topic.Title,
                Description = topic.Description,
                TopicId = id,
                Posts = dataContext
                .Posts
                .Include(p => p.Author)
                .Include(p => p.Reply)
                .Where(p => p.DeletedDt == null && p.TopicId == topicId)
                .Select(p => new ForumPostViewModel()
                {
                    Content = p.Content,
                    AuthorName = p.Author.IsRealNamePublic ? p.Author.RealName : p.Author.Login,
                    AuthorAvatar = $"/avatars/{p.Author.Avatar ?? "no-avatar.png"}"
                })
                .ToList()
            };
            if (HttpContext.Session.GetString("CreatePostMesssage") is String message)
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
                        Content = HttpContext.Session.GetString("SavedContent")!,
                        ReplyId = HttpContext.Session.GetString("SavedReplyId")!
                    };
                    HttpContext.Session.Remove("SavedContent");
                    HttpContext.Session.Remove("SavedReplyId");
                }
                HttpContext.Session.Remove("CreatePostMesssage");
                HttpContext.Session.Remove("isMessagePositive");
            }
            return View(model);
        }
        public IActionResult Themes([FromRoute] String id)
        {
            Guid themeId;
            try
            {
                themeId = Guid.Parse(id);
            }
            catch
            {
                themeId = Guid.Empty;//dataContext.Sections.First(s => s.UrlId == id).Id;
            }
            var theme = dataContext.Themes.Find(themeId);
            if (theme == null)
            {
                return NotFound();
            }
            ForumThemesModel model = new()
            {
                UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
                Title = theme.Title,
                ThemeId = id,
                Topics = dataContext
                .Topics
                .Include(t=>t.Author)
                .Where(t => t.DeletedDt == null && t.ThemeId == themeId)
                .AsEnumerable()
                .Select(t => new ForumTopicViewModel()
                {
                    Title = t.Title,
                    Description = (t.Description.Length > 100 
                    ?t.Description[..100]+ "..." : t.Description),
                    UrlIdString = t.Id.ToString(),
                    CreatedDtString = DateTime.Today == t.CreatedDt.Date
                            ? "Сьогодні "
                            : t.CreatedDt.ToString("dd.MM.yyyy HH:mm"),
                    Author = t.Author,
                    AuthorName = t.Author.IsRealNamePublic
                            ? t.Author.RealName
                            : t.Author.Login,
                    AuthorAvatarUrl = $"/avatars/{t.Author.Avatar ?? "no-avatar.png"}",
                    AuthorRegistrationDate = t.Author.RegisterDt.ToString("dd:MM:yyyy")
                })
                .ToList()
            };
            if (HttpContext.Session.GetString("CreateTopicMesssage") is String message)
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
                HttpContext.Session.Remove("CreateTopicMesssage");
                HttpContext.Session.Remove("isMessagePositive");
            }
            return View(model);
        }
        public ViewResult Sections([FromRoute] String id)
        {
            Guid sectionId;
            try
            {
                sectionId = Guid.Parse(id);
            }
            catch
            {
                sectionId = dataContext.Sections.First(s => s.UrlId == id).Id;
            }
            String? userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            ForumSectionsModel model = new()
            {
                UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
                SectionId = sectionId.ToString(),
                Themes = dataContext
                    .Themes
                    .Include(t => t.Author)
                    .Include(t => t.RateList)
                    .Where(t => t.DeletedDt == null && t.SectionId == sectionId)
                    .AsEnumerable()
                    .Select(t => new ForumThemeViewModel()
                    {
                        Title = t.Title,
                        Description = t.Description,
                        CreatedDtString = DateTime.Today == t.CreatedDt.Date
                            ? "Сьогодні "
                            : t.CreatedDt.ToString("dd.MM.yyyy HH:mm"),
                        UrlIdString = t.Id.ToString(),
                        SectionId = t.SectionId.ToString(),
                        Author = t.Author,
                        AuthorName = t.Author.IsRealNamePublic
                            ? t.Author.RealName
                            : t.Author.Login,
                        AuthorAvatarUrl = $"/avatars/{t.Author.Avatar ?? "no-avatar.png"}",
                        AuthorRegistrationDate = t.Author.RegisterDt.ToString("dd:MM:yyyy"),
                        //rating data
                        LikesCount = t.RateList.Count(r => r.Rating > 0),
                        DislikesCount = t.RateList.Count(r => r.Rating < 0),
                        GivenRating = userId == null ? null
                        : t.RateList.FirstOrDefault(r => r.UserId == Guid.Parse(userId))?.Rating
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
        public RedirectToActionResult CreateTopic(ForumTopicFormModel formModel)
        {
            if (!validationService.Validate(formModel.Title, ValidationTerms.NotEmpty))
            {
                HttpContext.Session.SetString("CreateTopicMesssage",
                    "Назва не може бути порожньою");
                HttpContext.Session.SetInt32("isMessagePositive", 0);
                HttpContext.Session.SetString("SavedTitle", formModel.Title ?? String.Empty);
                HttpContext.Session.SetString("SavedDescription", formModel.Description ?? String.Empty);
            }
            else if (!validationService.Validate(formModel.Description, ValidationTerms.NotEmpty))
            {
                HttpContext.Session.SetString("CreateTopicMesssage",
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
                    String trans = transliterateService.Transliterate(formModel.Title);
                    String urlId = trans;
                    int n = 2;
                    while (dataContext.Sections.Any(s => s.UrlId == urlId))
                    {
                        urlId = $"{trans}{n++}";
                    }

                    dataContext.Topics.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        Title = formModel.Title,
                        Description = formModel.Description,
                        CreatedDt = DateTime.Now,
                        AuthorId = userId,
                        ThemeId=Guid.Parse(formModel.ThemeId)

                    });
                    dataContext.SaveChanges();
                    HttpContext.Session.SetString("CreateTopicMesssage",
                        "Додано успішно");
                    HttpContext.Session.SetInt32("isMessagePositive", 1);

                }
                catch
                {
                    HttpContext.Session.SetString("CreateTopicMesssage",
                     "Відмовлено в авторизації");
                    HttpContext.Session.SetInt32("isMessagePositive", 0);
                    HttpContext.Session.SetString("SavedTitle", formModel.Title ?? String.Empty);
                    HttpContext.Session.SetString("SavedDescription", formModel.Description ?? String.Empty);
                }

            }
            return RedirectToAction(nameof(Themes), new {id=formModel.ThemeId});
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
                    String trans = transliterateService.Transliterate(forumModel.Title);
                    String urlId = trans;
                    String savedName = "";
                    int n = 2;
                    while(dataContext.Sections.Any(s => s.UrlId == urlId))
                    {
                        urlId = $"{trans}{n++}";
                    }

                    if (forumModel.SectionLogo is not null) //є файл
                    {
                        String path = "wwwroot/img/logos/";
                        savedName = randomService.GeneratePhotoName(forumModel.SectionLogo.FileName, path);
                        String resultPath = path + savedName;
                        using FileStream fs = new(resultPath, FileMode.Create);
                        forumModel.SectionLogo.CopyTo(fs);
                    }

                    dataContext.Sections.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        Title = forumModel.Title,
                        Description = forumModel.Description,
                        CreatedDt = DateTime.Now,
                        AuthorId = userId,
                        UrlId=urlId,
                        SectionLogo=savedName

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
            return RedirectToAction(nameof(Sections), new { id = formModel.SectionId }); // TODO: id = UrlId ?? SectionId
        }
        [HttpPost]
        public RedirectToActionResult CreatePost(ForumPostFormModel formModel)
        {
            if (!validationService.Validate(formModel.Content, ValidationTerms.NotEmpty))
            {
                HttpContext.Session.SetString("CreatePostMesssage",
                    "Відповідь не може бути порожньою");
                HttpContext.Session.SetInt32("isMessagePositive", 0);
                HttpContext.Session.SetString("SavedContent", formModel.Content ?? String.Empty);
                HttpContext.Session.SetString("SavedReplyId", formModel.ReplyId ?? String.Empty);
            }
            else
            {
                Guid userId;
                try
                {
                    userId = Guid.Parse(
                        HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value
                        );
                    dataContext.Posts.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        Content = formModel.Content,
                        ReplyId = String.IsNullOrEmpty(formModel.ReplyId)
                            ? null
                            : Guid.Parse(formModel.ReplyId),
                        CreatedDt = DateTime.Now,
                        AuthorId = userId,
                        TopicId = Guid.Parse(formModel.TopicId)
                    });
                    dataContext.SaveChanges();
                    HttpContext.Session.SetString("CreatePostMesssage",
                        "Додано успішно");
                    HttpContext.Session.SetInt32("isMessagePositive", 1);

                }
                catch
                {
                    HttpContext.Session.SetString("CreatePostMesssage",
                     "Відмовлено в авторизації");
                    HttpContext.Session.SetInt32("isMessagePositive", 0);
                    HttpContext.Session.SetString("SavedContent", formModel.Content ?? String.Empty);

                }

            }
            return RedirectToAction(
                nameof(Topics), 
                new { id = formModel.TopicId });
        }
    }
}

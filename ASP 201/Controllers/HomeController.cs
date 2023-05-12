using ASP_201.Data;
using ASP_201.Models;
using ASP_201.Models.Home;
using ASP_201.Services;
using ASP_201.Services.Hash;
using ASP_201.Services.Random;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ASP_201.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DateService _dateService;
        private readonly TimeService _timeService;
        private readonly StampService _stampService;
        private readonly IHashService _hashService;
        private readonly DataContext dataContext;
        private readonly IRandomService randomService;
        private readonly IConfiguration configuration;

        public HomeController(ILogger<HomeController> logger,
                              DateService dateService,
                              TimeService timeService,
                              StampService stampService,
                              IHashService hashService,
                              DataContext dataContext,
                              IRandomService randomService,
                              IConfiguration configuration)
        {
            _logger = logger;
            _dateService = dateService;
            _timeService = timeService;
            _stampService = stampService;
            _hashService = hashService;
            this.dataContext = dataContext;
            this.randomService = randomService;
            this.configuration = configuration;
        }

        public IActionResult WebApi()
        {
            return View();
        }
        public ViewResult EmailConfirmation()
        {
            // дістаємо параметр з конфігураціїї
            var smtpConfig = new SmtpConfig()
            {
                Host = configuration["Smtp:Gmail:Host"] ?? "",
                Port = int.Parse(configuration["Smtp:Gmail:Port"] ?? "-1"),
                Email = configuration["Smtp:Gmail:Email"] ?? "",
                Ssl = bool.Parse(configuration["Smtp:Gmail:Ssl"] ?? "false")
            };
            return View(smtpConfig);
        }
        public ViewResult Sessions([FromQuery(Name = "session-attr")] String? sessionAttr)
        {
            if (sessionAttr is not null)
                HttpContext.Session.SetString("session-attribute", sessionAttr);

            return View();
        }
        public ViewResult Middleware()
        {
            return View();
        } 
        public ViewResult Context()
        {
            ViewData["UsersCount"] = dataContext.Users.Count();
            ViewData["EmailCode"] = randomService.ConfirmCode(6);
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult TagHelpers()
        {
            return View();
        }
        public IActionResult UrlPage()
        {
            return View();
        }
        public IActionResult ProductsTable()
        {
            Models.Home.PassDataModel model = new()
            {
                Header = "Products table",
                Title = "Products table",
                Products = new()
                {
                    new(){Name="Зарядний кабель",       Price=210    , Image="pic1.png" },
                    new(){Name="Маніпулятор \'миша\'",  Price=399.50 , Image="pic2.png" },
                    new(){Name="Наліпка\'Smiley\'",     Price=2.95   , Image="pic2.png" },
                    new(){Name="Серветки для монітору", Price=100    , Image="pic1.png" },
                    new(){Name="USB ліхтарик",          Price=49.50  , Image="pic1.png" },
                    new(){Name="Аккумулятор",           Price=280     },
                    new(){Name="OC Windows Home",       Price=1250    },
                }
            };
            return View(model);
        }
        public IActionResult DisplayTemplates()
        {
            Models.Home.PassDataModel model = new()
            {
                Header = "Шаблони",
                Title = "Шаблони відображення даних",
                Products = new()
                {
                    new(){Name="Зарядний кабель",       Price=210    , Image="pic1.png" },
                    new(){Name="Маніпулятор \'миша\'",  Price=399.50 , Image="pic2.png" },
                    new(){Name="Наліпка\'Smiley\'",     Price=2.95   , Image="pic2.png" },
                    new(){Name="Серветки для монітору", Price=100    , Image="pic1.png" },
                    new(){Name="USB ліхтарик",          Price=49.50  , Image="pic1.png" },
                    new(){Name="Аккумулятор",           Price=280     },
                    new(){Name="OC Windows Home",       Price=1250    },
                }
            };
            return View(model);
        }
        public IActionResult PassData()
        {
            Models.Home.PassDataModel model = new()
            {
                Header = "Моделі",
                Title = "Моделі передачі даних",
                Products = new()
                {
                    new(){Name="Зарядний кабель",       Price=210    },
                    new(){Name="Маніпулятор \'миша\'",  Price=399.50 },
                    new(){Name="Наліпка\'Smiley\'",     Price=2.95   },
                    new(){Name="Серветки для монітору", Price=100    },
                    new(){Name="USB ліхтарик",          Price=49.50  },
                    new(){Name="Аккумулятор",           Price=280    },
                    new(){Name="OC Windows Home",       Price=1250   },
                }
            };
            return View(model);
        }
        public IActionResult Razor()
        {
            return View();
        }
        public IActionResult Intro()
        {
            return View();
        }
        public IActionResult Scheme()
        {
            ViewBag.data = "Data in ViewBag";       // Спосіб передачі даних
            ViewData["data"] = "Data in ViewData";  // до представлення
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public ViewResult Services()
        {
            ViewData["date_service"] = _dateService.GetMoment();
            ViewData["date_hashcode"] = _dateService.GetHashCode();

            ViewData["time_service"] = _timeService.GetMoment();
            ViewData["time_hashcode"] = _timeService.GetHashCode();

            ViewData["stamp_service"] = _stampService.GetMoment();
            ViewData["stamp_hashcode"] = _stampService.GetHashCode();

            ViewData["hash_service"] = _hashService.Hash("123");

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
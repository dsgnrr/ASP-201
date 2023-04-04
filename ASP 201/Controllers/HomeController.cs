using ASP_201.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ASP_201.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
using ASP_201.Data;
using ASP_201.Models.Forum;
using Microsoft.AspNetCore.Mvc;

namespace ASP_201.Controllers
{
    public class ForumController:Controller
    {
        private readonly DataContext dataContext;

        public ForumController(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public IActionResult Index()
        {

            ForumIndexModel model = new()
            {
                UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
                Sections = dataContext.Sections.Where(s=>s.DeletedDt == null).ToList()
            };
            return View(model);
        }
        [HttpPost]
        public RedirectToActionResult CreateSection()
        {
            return RedirectToAction(nameof(Index));
        }
    }
}

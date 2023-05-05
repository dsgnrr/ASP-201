using Microsoft.AspNetCore.Mvc;

namespace ASP_201.Models.Forum
{
    public class ForumThemeFormModel
    {
        [FromForm(Name = "theme-title")]
        public string Title { get; set; } = null!;
        [FromForm(Name = "theme-description")]
        public string Description { get; set; } = null!;
        [FromForm(Name = "section-id")]
        public string SectionId { get; set; } = null!;
    }
}

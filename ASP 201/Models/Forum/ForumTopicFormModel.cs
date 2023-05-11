using Microsoft.AspNetCore.Mvc;

namespace ASP_201.Models.Forum
{
    public class ForumTopicFormModel
    {
        [FromForm(Name = "topic-title")]
        public string Title { get; set; } = null!;
        [FromForm(Name = "topic-description")]
        public string Description { get; set; } = null!;
        [FromForm(Name = "theme-id")]
        public string ThemeId { get; set; } = null!;
    }
}

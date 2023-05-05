namespace ASP_201.Models.Forum
{
    public class ForumSectionsModel
    {
        public Boolean UserCanCreate { get; set; }
        public String SectionId { get; set; } = null!;
        public List<ForumThemeViewModel> Themes { get; set; } = null!;

        // Дані для створення нової секції
        public String? CreateMessage { get; set; }
        public Boolean? IsMessagePositive { get; set; }
        public ForumThemeFormModel formModel { get; set; } = null!;
    }
}

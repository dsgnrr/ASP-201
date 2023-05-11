namespace ASP_201.Models.Forum
{
    public class ForumThemesModel
    {
        public Boolean UserCanCreate { get; set; }
        public String ThemeId { get; set; } = null!;
        public String Title { get; set; } = null!;
        public List<ForumTopicViewModel> Topics { get; set; } = null!;

        // Дані для створення нового топіку
        public String? CreateMessage { get; set; }
        public Boolean? IsMessagePositive { get; set; }
        public ForumTopicFormModel formModel { get; set; } = null!;
    }
}

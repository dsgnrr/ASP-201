namespace ASP_201.Models.Forum
{
    public class ForumIndexModel
    {
        public List<Data.Entity.Section> Sections { get; set; } = null!;
        public Boolean UserCanCreate { get; set; }

        // Дані для створення нової секції
        public String? CreateMessage { get; set; }
        public Boolean? IsMessagePositive { get; set;}
        public ForumSectionModel? formModel { get; set;}
    }
}

namespace ASP_201.Models.Forum
{
    public class ForumIndexModel
    {
        public List<Data.Entity.Section> Sections { get; set; } = null!;
        public Boolean UserCanCreate { get; set; }

    }
}

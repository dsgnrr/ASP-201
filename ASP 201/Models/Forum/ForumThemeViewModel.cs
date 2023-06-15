namespace ASP_201.Models.Forum
{
    public class ForumThemeViewModel
    {
        public String Title { get; set; } = null!;
        public String Description { get; set; } = null!;
        public String UrlIdString { get; set; } = null!;
        public String SectionId { get; set; } = null!;
        public String CreatedDtString { get; set; } = null!;

        //Author
        public String AuthorName { get; set; } = null!;
        public String AuthorAvatarUrl { get; set; } = null!;
        public String AuthorRegistrationDate { get; set; } = null!;

        public Data.Entity.User Author { get; set; } = null!;

        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }
        public int? GivenRating { get; set; }

    }
}

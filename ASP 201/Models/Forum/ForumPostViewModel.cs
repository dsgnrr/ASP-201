namespace ASP_201.Models.Forum
{
    public class ForumPostViewModel
    {
        public String Content { get; set; } = null!;

        //Author data
        public String AuthorName { get; set; } = null!;
        public String AuthorAvatar { get; set; } = null!;
    }
}

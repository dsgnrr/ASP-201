namespace ASP_201.Data.Entity
{
    public class Post
    {
        public Guid         Id          { get; set; }
        public Guid         TopicId     { get; set; }
        public Guid         AuthorId    { get; set; }
        public Guid?        ReplyId     { get; set; }
        public String       Content     { get; set; } = null!;
        public DateTime     CreatedDt   { get; set; }
        public DateTime?    DeletedDt   { get; set; }

        public User         Author      { get; set; } = null!;
        public Post?        Reply       { get; set; }
    }
}

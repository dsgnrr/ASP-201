namespace ASP_201.Data.Entity
{
    public class Topic
    {
        public Guid     Id          { get; set; }
        public Guid     ThemeId     { get; set; }
        public String   Title       { get; set; } = null!;
        public String   Description { get; set; } = null!;
        public Guid     AuthorId    { get; set; }
        public DateTime CreatedDt   { get; set; }
        public DateTime? DeletedDt   { get; set; }

        //Author data
        public User Author { get; set; } = null!;
    }
}

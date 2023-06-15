namespace ASP_201.Data.Entity
{
    public class Theme
    {
        public Guid Id { get; set; }
        public Guid SectionId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public Guid AuthorId { get; set; }
        public DateTime CreatedDt { get; set; }
        public DateTime? DeletedDt { get; set; }

        // Navigation
        public User Author { get; set; } = null!;
        public List<Rate> RateList { get; set; } = null!;
    }
}

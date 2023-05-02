using Microsoft.EntityFrameworkCore;

namespace ASP_201.Data
{
    public class DataContext:DbContext
    {   
        public DbSet<Entity.User>      Users { get; set; }
        public DbSet<Entity.Section>   Sections { get; set; }
        public DbSet<Entity.Theme>     Themes { get; set; }
        public DbSet<Entity.Topic>     Topics { get; set; }
        public DbSet<Entity.Rate>      Rates { get; set; }
        public DbSet<Entity.Post>      Posts { get; set; }
        public DbSet<Entity.EmailConfirmToken> EmailConfirmTokens { get; set; }

        public DataContext(DbContextOptions options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity.Rate>()
                .HasKey(
                    nameof(Entity.Rate.ItemId), // встановлення композитного  
                    nameof(Entity.Rate.UserId));// ключа
        }

    }
}

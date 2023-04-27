using Microsoft.EntityFrameworkCore;

namespace ASP_201.Data
{
    public class DataContext:DbContext
    {   
        public DbSet<Entity.User> Users { get; set; }
        public DbSet<Entity.EmailConfirmToken> EmailConfirmTokens { get; set; }

        public DataContext(DbContextOptions options) : base(options)
        {
        }

    }
}

using Microsoft.EntityFrameworkCore;

namespace UrlShortener.Entities;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<TinyUrl> TinyUrls { set; get; }
}
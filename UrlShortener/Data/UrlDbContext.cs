using Microsoft.EntityFrameworkCore;

namespace UrlShortener.Data;

public class UrlDbContext : DbContext
{
    public UrlDbContext(DbContextOptions<UrlDbContext> options)
    :base(options)
    {
    }

    public DbSet<Url> Urls => Set<Url>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UrlDbContext).Assembly);
    }
}
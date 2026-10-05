using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace UrlShortener.Data;

public class UrlRepository : IUrlRepository
{
    private readonly UrlDbContext _context;
    public UrlRepository(UrlDbContext context)
    {
        _context = context;
    }

    public async Task<Url> CreateAsync(Url entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        cancellationToken.ThrowIfCancellationRequested();

        _context.Urls.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public Task<bool> AnyAsync(string shortCode, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(shortCode);

        return _context.Urls.AnyAsync(url => url.shortCode == shortCode, cancellationToken);
    }

    public Task<Url?> GetByCodeAsync(string code, CancellationToken cencellationToken= default)
    {
        return _context.Urls.FirstOrDefaultAsync(url => url.shortCode == code,cencellationToken);
    }
}

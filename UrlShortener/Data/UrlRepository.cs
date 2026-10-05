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

        var existing = await _context.Urls.FirstOrDefaultAsync(
            url => url.longUrl == entity.longUrl, cancellationToken);
        if (existing is not null)
            return existing;

        _context.Urls.Add(entity);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException error) when (
            error.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 })
        {
            _context.Entry(entity).State = EntityState.Detached;
            existing = await _context.Urls.FirstOrDefaultAsync(
                url => url.longUrl == entity.longUrl, cancellationToken);
            if (existing is not null)
                return existing;

            if (await AnyAsync(entity.shortCode, cancellationToken))
                throw new DuplicateShortCodeException(entity.shortCode, error);

            throw;
        }
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

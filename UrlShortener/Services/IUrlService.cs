using UrlShortener.Data;

namespace UrlShortener.Services;

public interface IUrlService
{
    Task<Url> CreateAsync(string longUrl, CancellationToken cancellationToken = default);

    Task<Url?> GetByCodeAsync(string shortCode, CancellationToken cancellationToken = default);
}

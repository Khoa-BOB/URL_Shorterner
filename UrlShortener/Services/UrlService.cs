using UrlShortener.Core;
using UrlShortener.Data;

namespace UrlShortener.Services;

public class UrlService : IUrlService
{
    private const int MaxAttempts = 10;
    private readonly IUrlRepository _repository;

    public UrlService(IUrlRepository repository)
    {
        _repository = repository;
    }

    public async Task<Url> CreateAsync(string longUrl, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(longUrl);

        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var entity = new Url(longUrl, ShortCodeGenerator.GenerateShortCode());

            try
            {
                return await _repository.CreateAsync(entity, cancellationToken);
            }
            catch (DuplicateShortCodeException)
            {
                // The database enforces uniqueness, including concurrent requests.
            }
        }

        throw new InvalidOperationException(
            $"Could not generate a unique short code after {MaxAttempts} attempts.");
    }

    public async Task<Url?> GetByCodeAsync(string shortCode, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByCodeAsync(shortCode, cancellationToken);
    }
}

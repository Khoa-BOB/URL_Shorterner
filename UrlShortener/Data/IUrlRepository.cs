using UrlShortener.Data;

public interface IUrlRepository
{
    Task<Url> CreateAsync(Url entity, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(string shortCode, CancellationToken cancellationToken = default);

    Task<Url?> GetByCodeAsync(string code, CancellationToken cencellationToken= default);
}

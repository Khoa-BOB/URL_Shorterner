namespace UrlShortener.Data;

public sealed class DuplicateShortCodeException : Exception
{
    public DuplicateShortCodeException(string shortCode, Exception innerException)
        : base($"The short code '{shortCode}' already exists.", innerException)
    {
    }
}

namespace UrlShortener.Data;

public class Url
{
    private Url() { }

    public Url(string longUrl, string shortCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(longUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(shortCode);

        if (shortCode.Length != Core.ShortCodeGenerator.ShortCodeLength)
            throw new ArgumentException("Short codes must contain exactly seven characters.", nameof(shortCode));

        this.longUrl = longUrl;
        this.shortCode = shortCode;
    }

    public int Id {get; private set;}
    public string shortCode {get; private set;} = null!;

    public string longUrl {get; private set;} = null!;

}

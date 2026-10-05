namespace UrlShortener.Contract;

public record CreateShortUrlRequest(
    string longUrl
);

public record ShortUrlResponse(
    int Id,
    string shortUrl,
    string longUrl
);



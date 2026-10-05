using Microsoft.EntityFrameworkCore;
using UrlShortener.Contract;
using UrlShortener.Data;
using UrlShortener.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<UrlDbContext>(options =>
    options.UseSqlite(connectionString));


builder.Services.AddScoped<IUrlRepository, UrlRepository>();
builder.Services.AddScoped<IUrlService, UrlService>();


var app = builder.Build();

app.MapPost("/url/shorten", async (
    CreateShortUrlRequest body,
    IUrlService urls,
    HttpRequest request,
    CancellationToken cancellationToken
) =>
{
    if (!Uri.TryCreate(body.longUrl, UriKind.Absolute, out var longUrl) ||
        (longUrl.Scheme != Uri.UriSchemeHttp && longUrl.Scheme != Uri.UriSchemeHttps))
    {
        return Results.BadRequest("Provide a valid absolute HTTP or HTTPS URL.");
    }

    var entity = await urls.CreateAsync(body.longUrl, cancellationToken);
    var shortUrl = $"{request.Scheme}://{request.Host}{request.PathBase}/{entity.shortCode}";

    return Results.Ok(new ShortUrlResponse(entity.Id, shortUrl, entity.longUrl));
});

app.MapGet("/{code}", async (
    string code,
    IUrlService urls,
    CancellationToken cancellationToken
) =>
{   
    var url = await urls.GetByCodeAsync(code, cancellationToken);

    if (url is null)
    {
        return Results.NotFound();
    }

    return Results.Redirect(url.longUrl);
    
});

app.Run();

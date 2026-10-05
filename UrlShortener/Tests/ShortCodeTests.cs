using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using UrlShortener.Core;
using UrlShortener.Data;
using UrlShortener.Services;
using Xunit;

namespace UrlShortener.Tests;

public class ShortCodeTests
{
    [Fact]
    public void GeneratorAlwaysReturnsConfiguredLengthBase62Characters()
    {
        for (var i = 0; i < 10_000; i++)
            Assert.Matches($"^[a-zA-Z0-9]{{{ShortCodeGenerator.ShortCodeLength}}}$", ShortCodeGenerator.GenerateShortCode());
    }

    [Fact]
    public async Task RepeatedLongUrlReturnsTheExistingLink()
    {
        var interceptor = new InsertInterceptor();
        await using var database = await TestDatabase.CreateAsync(interceptor);
        var service = new UrlService(new UrlRepository(database.Context));

        var first = await service.CreateAsync("https://example.com/new");
        var second = await service.CreateAsync("https://example.com/new");

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(first.shortCode, second.shortCode);
        Assert.Equal(1, interceptor.Attempts);
        Assert.Equal(2, await database.Context.Urls.CountAsync());
    }

    [Fact]
    public async Task CompetingInsertReturnsTheWinningLink()
    {
        var interceptor = new InsertInterceptor();
        await using var database = await TestDatabase.CreateAsync(interceptor);
        var winningCode = new string('b', ShortCodeGenerator.ShortCodeLength);
        interceptor.BeforeInsert = async () =>
        {
            await using var competingContext = database.CreateContext();
            competingContext.Urls.Add(new Url("https://example.com/new", winningCode));
            await competingContext.SaveChangesAsync();
        };

        var saved = await new UrlService(new UrlRepository(database.Context))
            .CreateAsync("https://example.com/new");

        Assert.True(saved.Id > 0);
        Assert.Equal(winningCode, saved.shortCode);
        Assert.Equal(saved.Id, (await database.Context.Urls.SingleAsync(
            url => url.longUrl == "https://example.com/new")).Id);
        Assert.Equal(1, interceptor.Attempts);
        Assert.Equal(2, await database.Context.Urls.CountAsync());
        Assert.DoesNotContain(database.Context.ChangeTracker.Entries<Url>(),
            entry => entry.State == EntityState.Added);
    }

    [Fact]
    public async Task DatabaseRejectsDuplicateLongUrlsDirectly()
    {
        await using var database = await TestDatabase.CreateAsync(new InsertInterceptor());
        database.Context.Urls.Add(new Url("https://example.com/original",
            new string('b', ShortCodeGenerator.ShortCodeLength)));

        var error = await Assert.ThrowsAsync<DbUpdateException>(() => database.Context.SaveChangesAsync());

        Assert.Equal(2067, Assert.IsType<SqliteException>(error.InnerException).SqliteExtendedErrorCode);
        Assert.Equal(1, await database.Context.Urls.CountAsync());
    }

    [Fact]
    public async Task DistinctLongUrlStringsHaveDistinctLinks()
    {
        await using var database = await TestDatabase.CreateAsync(new InsertInterceptor());
        var service = new UrlService(new UrlRepository(database.Context));
        string[] urls = ["https://example.com/path", "https://example.com/Path",
            "https://example.com/path?q=1", "https://example.com/path?q=2"];
        var ids = new HashSet<int>();

        foreach (var longUrl in urls)
        {
            var saved = await service.CreateAsync(longUrl);
            Assert.Equal(longUrl, saved.longUrl);
            Assert.True(ids.Add(saved.Id));
        }

        Assert.Equal(5, await database.Context.Urls.CountAsync());
    }

    [Fact]
    public async Task RepositoryPersistsTheSuppliedShortCode()
    {
        await using var database = await TestDatabase.CreateAsync(new InsertInterceptor());
        var code = new string('b', ShortCodeGenerator.ShortCodeLength);
        var entity = new Url("https://example.com/new", code);
        var repository = new UrlRepository(database.Context);

        var saved = await repository.CreateAsync(entity);

        Assert.Same(entity, saved);
        Assert.Equal(code, saved.shortCode);
        Assert.True(await repository.AnyAsync(code));
        Assert.False(await repository.AnyAsync(new string('c', ShortCodeGenerator.ShortCodeLength)));
    }

    [Fact]
    public async Task RepositoryReportsACollisionWithoutRetrying()
    {
        var interceptor = new InsertInterceptor();
        await using var database = await TestDatabase.CreateAsync(interceptor);
        var entity = new Url("https://example.com/new", new string('a', ShortCodeGenerator.ShortCodeLength));

        var error = await Assert.ThrowsAsync<DuplicateShortCodeException>(() =>
            new UrlRepository(database.Context).CreateAsync(entity));

        Assert.IsType<DbUpdateException>(error.InnerException);
        Assert.Equal(1, interceptor.Attempts);
        Assert.Equal(EntityState.Detached, database.Context.Entry(entity).State);
        Assert.Equal(1, await database.Context.Urls.CountAsync());
    }

    [Fact]
    public async Task RepeatedCollisionsRetryWithoutChangingTheExistingRecord()
    {
        var interceptor = new InsertInterceptor(collisions: 2);
        await using var database = await TestDatabase.CreateAsync(interceptor);
        var url = await new UrlService(new UrlRepository(database.Context)).CreateAsync("https://example.com/new");

        Assert.Equal(3, interceptor.Attempts);
        Assert.NotEqual(new string('a', ShortCodeGenerator.ShortCodeLength), url.shortCode);
        Assert.Matches($"^[a-zA-Z0-9]{{{ShortCodeGenerator.ShortCodeLength}}}$", url.shortCode);
        Assert.True(url.Id > 0);
        Assert.Equal(2, await database.Context.Urls.CountAsync());
        Assert.Equal("https://example.com/original",
            (await database.Context.Urls.SingleAsync(u => u.shortCode == new string('a', ShortCodeGenerator.ShortCodeLength))).longUrl);
        Assert.DoesNotContain(database.Context.ChangeTracker.Entries<Url>(),
            entry => entry.State == EntityState.Added);
    }

    [Fact]
    public async Task TenCollisionsFailWithoutStoringADuplicate()
    {
        var interceptor = new InsertInterceptor(collisions: 10);
        await using var database = await TestDatabase.CreateAsync(interceptor);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new UrlService(new UrlRepository(database.Context)).CreateAsync("https://example.com/new"));

        Assert.Equal(10, interceptor.Attempts);
        Assert.Equal(1, await database.Context.Urls.CountAsync());
        Assert.DoesNotContain(database.Context.ChangeTracker.Entries<Url>(),
            entry => entry.State == EntityState.Added);
    }

    [Fact]
    public async Task OtherDatabaseErrorsAreNotRetried()
    {
        var interceptor = new InsertInterceptor(invalidateLongUrl: true);
        await using var database = await TestDatabase.CreateAsync(interceptor);

        var error = await Assert.ThrowsAsync<DbUpdateException>(() =>
            new UrlService(new UrlRepository(database.Context)).CreateAsync("https://example.com/new"));

        Assert.Equal(1299, Assert.IsType<SqliteException>(error.InnerException).SqliteExtendedErrorCode);
        Assert.Equal(1, interceptor.Attempts);
        Assert.Equal(1, await database.Context.Urls.CountAsync());
    }

    [Fact]
    public async Task CancelledRequestDoesNotInsertAnything()
    {
        var interceptor = new InsertInterceptor();
        await using var database = await TestDatabase.CreateAsync(interceptor);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new UrlService(new UrlRepository(database.Context)).CreateAsync("https://example.com/new", cancellation.Token));

        Assert.Equal(0, interceptor.Attempts);
        Assert.Equal(1, await database.Context.Urls.CountAsync());
    }

    private sealed class InsertInterceptor(int collisions = 0, bool invalidateLongUrl = false)
        : SaveChangesInterceptor
    {
        public int Attempts { get; private set; }
        public Func<Task>? BeforeInsert { get; set; }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            Attempts++;
            if (BeforeInsert is not null)
                await BeforeInsert();
            var entry = eventData.Context!.ChangeTracker.Entries<Url>()
                .Single(e => e.State == EntityState.Added);
            if (Attempts <= collisions)
                entry.Property(u => u.shortCode).CurrentValue = new string('a', ShortCodeGenerator.ShortCodeLength);
            if (invalidateLongUrl)
                entry.Property(u => u.longUrl).CurrentValue = null!;
            return result;
        }
    }

    private sealed class TestDatabase(SqliteConnection connection, UrlDbContext context) : IAsyncDisposable
    {
        public UrlDbContext Context => context;

        public UrlDbContext CreateContext() => new(
            new DbContextOptionsBuilder<UrlDbContext>().UseSqlite(connection).Options);

        public static async Task<TestDatabase> CreateAsync(SaveChangesInterceptor interceptor)
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<UrlDbContext>().UseSqlite(connection).Options;
            await using (var setup = new UrlDbContext(options))
            {
                await setup.Database.EnsureCreatedAsync();
                setup.Urls.Add(new Url("https://example.com/original", new string('a', ShortCodeGenerator.ShortCodeLength)));
                await setup.SaveChangesAsync();
            }

            return new TestDatabase(connection, new UrlDbContext(
                new DbContextOptionsBuilder<UrlDbContext>().UseSqlite(connection)
                    .AddInterceptors(interceptor).Options));
        }

        public async ValueTask DisposeAsync()
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}

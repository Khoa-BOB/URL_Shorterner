using System.Security.Cryptography;

namespace UrlShortener.Core;
public static class ShortCodeGenerator
{
    public const int ShortCodeLength = 8;
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    // A candidate only: the database unique index guarantees stored uniqueness.
    public static string GenerateShortCode()
        => RandomNumberGenerator.GetString(Alphabet, ShortCodeLength);
}

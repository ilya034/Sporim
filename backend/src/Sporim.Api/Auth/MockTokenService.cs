using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Sporim.Api.Contracts;
using Sporim.Api.State;

namespace Sporim.Api.Auth;

public sealed class MockTokenService
{
    private static readonly byte[] SecretBytes = Encoding.UTF8.GetBytes("sporim-mock-api-development-secret-key-2026");

    public string IssueToken(UserEntity user)
    {
        var headerJson = """{"alg":"HS256","typ":"JWT"}""";
        var payloadObj = new Dictionary<string, object?>
        {
            ["sub"] = user.Id.ToString(),
            ["unique_name"] = user.Username,
            ["name"] = user.DisplayName,
            ["email"] = user.Email,
            ["role"] = user.Role.ToString().ToLowerInvariant(),
            ["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ["exp"] = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds()
        };

        var payloadJson = JsonSerializer.Serialize(payloadObj);
        var headerSegment = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));
        var payloadSegment = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
        var unsignedToken = $"{headerSegment}.{payloadSegment}";
        var signatureSegment = ComputeSignature(unsignedToken);

        return $"{unsignedToken}.{signatureSegment}";
    }

    public Guid? TryValidateAndGetUserId(string? authorizationHeader, MockStore store)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            return null;
        }

        const string bearerPrefix = "Bearer ";
        if (!authorizationHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var token = authorizationHeader[bearerPrefix.Length..].Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        // Built-in developer alias tokens for quick Swagger / curl testing
        if (string.Equals(token, "mock-user-token", StringComparison.Ordinal))
        {
            return MockStore.VanyaUserId;
        }

        if (string.Equals(token, "mock-moderator-token", StringComparison.Ordinal))
        {
            return MockStore.ModeratorUserId;
        }

        if (string.Equals(token, "mock-admin-token", StringComparison.Ordinal))
        {
            return MockStore.AdminUserId;
        }

        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            return null;
        }

        var unsignedToken = $"{parts[0]}.{parts[1]}";
        var expectedSignature = ComputeSignature(unsignedToken);
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(parts[2]),
                Encoding.UTF8.GetBytes(expectedSignature)))
        {
            return null;
        }

        try
        {
            var payloadBytes = Base64UrlDecode(parts[1]);
            using var doc = JsonDocument.Parse(payloadBytes);
            if (doc.RootElement.TryGetProperty("sub", out var subProp) &&
                Guid.TryParse(subProp.GetString(), out var userId) &&
                store.UserExists(userId))
            {
                return userId;
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    private static string ComputeSignature(string unsignedToken)
    {
        var hash = HMACSHA256.HashData(SecretBytes, Encoding.UTF8.GetBytes(unsignedToken));
        return Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var padded = input.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2:
                padded += "==";
                break;
            case 3:
                padded += "=";
                break;
        }

        return Convert.FromBase64String(padded);
    }
}

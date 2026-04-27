using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace UserAccess.Api.Services;

public sealed class InMemoryUserStore : IUserStore
{
    public const string DemoEmail = "demo@example.com";
    public const string DemoPassword = "ParkingLot123!";

    private static readonly Guid DemoUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    private readonly ConcurrentDictionary<string, UserAccount> _usersByEmail = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<Guid, UserAccount> _usersById = new();

    public InMemoryUserStore()
    {
        AddUser(DemoUserId, DemoEmail, DemoPassword, "Demo User");
    }

    public bool TryCreate(string email, string password, string displayName, out UserAccount user)
    {
        user = new UserAccount(
            Guid.NewGuid(),
            NormalizeEmail(email),
            displayName.Trim(),
            HashPassword(password));

        if (!_usersByEmail.TryAdd(user.Email, user))
        {
            return false;
        }

        _usersById[user.Id] = user;
        return true;
    }

    public bool TryValidateCredentials(string email, string password, out UserAccount user)
    {
        if (!_usersByEmail.TryGetValue(NormalizeEmail(email), out user!))
        {
            return false;
        }

        return VerifyPassword(password, user.PasswordHash);
    }

    public bool TryGetById(Guid userId, out UserAccount user)
        => _usersById.TryGetValue(userId, out user!);

    public bool TryGetByEmail(string email, out UserAccount user)
        => _usersByEmail.TryGetValue(NormalizeEmail(email), out user!);

    private void AddUser(Guid id, string email, string password, string displayName)
    {
        var user = new UserAccount(
            id,
            NormalizeEmail(email),
            displayName.Trim(),
            HashPassword(password));

        _usersByEmail[user.Email] = user;
        _usersById[user.Id] = user;
    }

    private static string NormalizeEmail(string email)
        => email.Trim().ToLowerInvariant();

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string passwordHash)
    {
        var parts = passwordHash.Split('.');
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[1]);
        var expectedHash = Convert.FromBase64String(parts[2]);
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}

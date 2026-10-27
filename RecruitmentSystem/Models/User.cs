using System.Security.Cryptography;

namespace RecruitmentSystem.Models;

/// <summary>Entity 1 (Users table) - a candidate or an HR administrator.</summary>
public class User
{
    private const int Iterations = 100_000;
    private byte[] _salt = Array.Empty<byte>();
    private byte[] _hash = Array.Empty<byte>();

    public int Id { get; set; }
    public string FullName { get; private set; }
    public string Email { get; private set; }
    public string Phone { get; private set; }
    public UserRole Role { get; private set; }
    public DateTime CreatedAt { get; } = DateTime.Now;

    public User(string fullName, string email, string phone, string password, UserRole role = UserRole.Candidate)
    {
        FullName = fullName.Trim();
        Email = email.Trim().ToLowerInvariant();
        Phone = phone.Trim();
        Role = role;
        SetPassword(password);
    }

    public void SetPassword(string password)
    {
        _salt = RandomNumberGenerator.GetBytes(16);
        _hash = Derive(password, _salt);
    }

    public bool VerifyPassword(string password) =>
        CryptographicOperations.FixedTimeEquals(_hash, Derive(password, _salt));

    public bool IsAdmin() => Role == UserRole.Admin;

    public void UpdateProfile(string fullName, string phone)
    {
        FullName = fullName.Trim();
        Phone = phone.Trim();
    }

    private static byte[] Derive(string password, byte[] salt) =>
        Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);
}

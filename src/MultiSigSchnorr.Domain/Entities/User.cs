using MultiSigSchnorr.Domain.Enums;

namespace MultiSigSchnorr.Domain.Entities;

public sealed class User
{
    public Guid Id { get; private set; }
    public string Login { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public DateTime CreatedUtc { get; private set; }

    public User(Guid id, string login, string passwordHash, UserRole role, DateTime createdUtc)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("User id cannot be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(login))
            throw new ArgumentException("Login cannot be empty.", nameof(login));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

        Id = id;
        Login = login.Trim();
        PasswordHash = passwordHash;
        Role = role;
        CreatedUtc = createdUtc;
    }

    private User() { }
}
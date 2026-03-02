namespace Chess.Server.Models;

// Template placeholder for auth user model.
public sealed class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int Rating { get; set; } = 1200;
    public DateTime CreatedAtUtc { get; set; }
}

using SQLite;

namespace FinanceManager.Models;

[Table("Users")]
public class UserProfile
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhotoUrl { get; set; } = string.Empty;

    public string AuthProvider { get; set; } = string.Empty; // "Google" or "Apple"

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

    public string Currency { get; set; } = "USD";
}

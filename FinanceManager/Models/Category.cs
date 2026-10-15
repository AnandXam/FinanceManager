using SQLite;

namespace FinanceManager.Models;

[Table("Categories")]
public class Category
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Icon { get; set; } = "💰";

    public string ColorHex { get; set; } = "#6C63FF";

    public TransactionType Type { get; set; }

    public bool IsDefault { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Ignore]
    public Color DisplayColor => Color.FromArgb(ColorHex);
}

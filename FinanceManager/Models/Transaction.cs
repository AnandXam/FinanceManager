using SQLite;

namespace FinanceManager.Models;

public enum TransactionType
{
    Income,
    Expense
}

[Table("Transactions")]
public class Transaction
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public TransactionType Type { get; set; }

    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public DateTime Date { get; set; } = DateTime.Now;

    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Ignore]
    public string FormattedAmount => Type == TransactionType.Income
        ? $"+${Amount:N2}"
        : $"-${Amount:N2}";

    [Ignore]
    public Color AmountColor => Type == TransactionType.Income
        ? Color.FromArgb("#4CAF50")
        : Color.FromArgb("#F44336");

    [Ignore]
    public string TypeIcon => Type == TransactionType.Income ? "↑" : "↓";

    [Ignore]
    public string FormattedDate => Date.ToString("MMM dd, yyyy");
}

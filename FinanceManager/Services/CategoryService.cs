using FinanceManager.Models;

namespace FinanceManager.Services;

public class CategoryService
{
    private readonly DatabaseService _databaseService;

    public CategoryService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task SeedDefaultCategoriesAsync(string userId)
    {
        var hasCategories = await _databaseService.HasCategoriesAsync(userId);
        if (hasCategories) return;

        var defaultCategories = GetDefaultCategories(userId);
        foreach (var category in defaultCategories)
        {
            await _databaseService.SaveCategoryAsync(category);
        }
    }

    public static List<Category> GetDefaultCategories(string userId)
    {
        return new List<Category>
        {
            // ── Expense Categories ─────────────────────────────
            new() { UserId = userId, Name = "Food & Dining", Icon = "🍕", ColorHex = "#FF6B6B", Type = TransactionType.Expense, IsDefault = true },
            new() { UserId = userId, Name = "Transportation", Icon = "🚗", ColorHex = "#4ECDC4", Type = TransactionType.Expense, IsDefault = true },
            new() { UserId = userId, Name = "Shopping", Icon = "🛍️", ColorHex = "#45B7D1", Type = TransactionType.Expense, IsDefault = true },
            new() { UserId = userId, Name = "Housing & Rent", Icon = "🏠", ColorHex = "#96CEB4", Type = TransactionType.Expense, IsDefault = true },
            new() { UserId = userId, Name = "Utilities", Icon = "💡", ColorHex = "#FFEAA7", Type = TransactionType.Expense, IsDefault = true },
            new() { UserId = userId, Name = "Healthcare", Icon = "🏥", ColorHex = "#DDA0DD", Type = TransactionType.Expense, IsDefault = true },
            new() { UserId = userId, Name = "Entertainment", Icon = "🎬", ColorHex = "#FF8A5C", Type = TransactionType.Expense, IsDefault = true },
            new() { UserId = userId, Name = "Education", Icon = "📚", ColorHex = "#A8E6CF", Type = TransactionType.Expense, IsDefault = true },
            new() { UserId = userId, Name = "Personal Care", Icon = "💅", ColorHex = "#FFB6C1", Type = TransactionType.Expense, IsDefault = true },
            new() { UserId = userId, Name = "Insurance", Icon = "🛡️", ColorHex = "#87CEEB", Type = TransactionType.Expense, IsDefault = true },
            new() { UserId = userId, Name = "Subscriptions", Icon = "📱", ColorHex = "#C39BD3", Type = TransactionType.Expense, IsDefault = true },
            new() { UserId = userId, Name = "Gifts & Donations", Icon = "🎁", ColorHex = "#F1948A", Type = TransactionType.Expense, IsDefault = true },
            new() { UserId = userId, Name = "Travel", Icon = "✈️", ColorHex = "#76D7C4", Type = TransactionType.Expense, IsDefault = true },
            new() { UserId = userId, Name = "Groceries", Icon = "🛒", ColorHex = "#F9E79F", Type = TransactionType.Expense, IsDefault = true },
            new() { UserId = userId, Name = "Other Expense", Icon = "📌", ColorHex = "#AEB6BF", Type = TransactionType.Expense, IsDefault = true },

            // ── Income Categories ──────────────────────────────
            new() { UserId = userId, Name = "Salary", Icon = "💼", ColorHex = "#4CAF50", Type = TransactionType.Income, IsDefault = true },
            new() { UserId = userId, Name = "Freelance", Icon = "💻", ColorHex = "#66BB6A", Type = TransactionType.Income, IsDefault = true },
            new() { UserId = userId, Name = "Investments", Icon = "📈", ColorHex = "#43A047", Type = TransactionType.Income, IsDefault = true },
            new() { UserId = userId, Name = "Business", Icon = "🏢", ColorHex = "#388E3C", Type = TransactionType.Income, IsDefault = true },
            new() { UserId = userId, Name = "Rental Income", Icon = "🏘️", ColorHex = "#2E7D32", Type = TransactionType.Income, IsDefault = true },
            new() { UserId = userId, Name = "Dividends", Icon = "💎", ColorHex = "#1B5E20", Type = TransactionType.Income, IsDefault = true },
            new() { UserId = userId, Name = "Side Hustle", Icon = "🚀", ColorHex = "#81C784", Type = TransactionType.Income, IsDefault = true },
            new() { UserId = userId, Name = "Bonus", Icon = "🎉", ColorHex = "#A5D6A7", Type = TransactionType.Income, IsDefault = true },
            new() { UserId = userId, Name = "Refund", Icon = "💵", ColorHex = "#C8E6C9", Type = TransactionType.Income, IsDefault = true },
            new() { UserId = userId, Name = "Other Income", Icon = "✨", ColorHex = "#E8F5E9", Type = TransactionType.Income, IsDefault = true },
        };
    }

    public static List<(string Name, string Icon, string Color)> GetSuggestedExpenseCategories()
    {
        return new List<(string, string, string)>
        {
            ("Pet Care", "🐾", "#FF9800"),
            ("Fitness & Gym", "🏋️", "#E91E63"),
            ("Coffee & Drinks", "☕", "#795548"),
            ("Books & Media", "📖", "#9C27B0"),
            ("Home Improvement", "🔧", "#607D8B"),
            ("Childcare", "👶", "#FF5722"),
            ("Parking & Tolls", "🅿️", "#455A64"),
            ("Laundry", "👕", "#00BCD4"),
        };
    }

    public static List<(string Name, string Icon, string Color)> GetSuggestedIncomeCategories()
    {
        return new List<(string, string, string)>
        {
            ("Commission", "💰", "#009688"),
            ("Tips", "🤑", "#8BC34A"),
            ("Royalties", "👑", "#FF9800"),
            ("Interest", "🏦", "#3F51B5"),
            ("Grants", "🎓", "#00BCD4"),
        };
    }
}

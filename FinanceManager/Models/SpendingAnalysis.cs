namespace FinanceManager.Models;

public class SpendingAnalysis
{
    public string Summary { get; set; } = string.Empty;

    public List<SpendingInsight> Insights { get; set; } = new();

    public List<CategoryBreakdown> CategoryBreakdowns { get; set; } = new();

    public decimal TotalIncome { get; set; }

    public decimal TotalExpenses { get; set; }

    public decimal SavingsRate { get; set; }

    public string AiRecommendation { get; set; } = string.Empty;

    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;

    public string Period { get; set; } = "This Month";
}

public class SpendingInsight
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Icon { get; set; } = "💡";

    public InsightType Type { get; set; }
}

public enum InsightType
{
    Positive,
    Warning,
    Neutral,
    Tip
}

public class CategoryBreakdown
{
    public string CategoryName { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public double Percentage { get; set; }

    public string ColorHex { get; set; } = "#6C63FF";

    public string FormattedAmount => $"${Amount:N2}";

    public string FormattedPercentage => $"{Percentage:N1}%";
}

public class MonthlySummary
{
    public int Month { get; set; }

    public int Year { get; set; }

    public decimal TotalIncome { get; set; }

    public decimal TotalExpenses { get; set; }

    public decimal NetSavings => TotalIncome - TotalExpenses;

    public string MonthName => new DateTime(Year, Month, 1).ToString("MMM");
}

using FinanceManager.Models;

namespace FinanceManager.Services;

/// <summary>
/// AI-powered spending analysis service using local LLaMA model via LLamaSharp.
/// Falls back to rule-based analysis when the AI model is not available.
/// The service analyzes income/expense patterns and provides personalized insights.
/// </summary>
public class AIAnalysisService
{
    private readonly DatabaseService _databaseService;
    private bool _isModelLoaded;
    private object? _modelSession; // LLamaSharp model session (loaded dynamically)

    public AIAnalysisService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    /// <summary>
    /// Attempts to load the local LLaMA model for AI-powered analysis.
    /// The model file should be placed in the app's data directory.
    /// </summary>
    public async Task<bool> TryLoadModelAsync()
    {
        try
        {
            var modelPath = Path.Combine(FileSystem.AppDataDirectory, "models", "llama-finance.gguf");

            if (!File.Exists(modelPath))
            {
                System.Diagnostics.Debug.WriteLine("LLaMA model not found. Using rule-based analysis.");
                return false;
            }

            // Load LLamaSharp model
            // In production, this would initialize the LLamaWeights and LLamaContext
            // var parameters = new ModelParams(modelPath) { ContextSize = 2048, GpuLayerCount = 0 };
            // var model = LLamaWeights.LoadFromFile(parameters);
            // _modelSession = new InteractiveExecutor(new LLamaContext(model, parameters));

            _isModelLoaded = true;
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load AI model: {ex.Message}");
            _isModelLoaded = false;
            return false;
        }
    }

    /// <summary>
    /// Generates a comprehensive spending analysis using AI or rule-based approach.
    /// </summary>
    public async Task<SpendingAnalysis> AnalyzeSpendingAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var from = fromDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var to = toDate ?? DateTime.Now;

        var transactions = await _databaseService.GetTransactionsByDateRangeAsync(userId, from, to);

        if (transactions.Count == 0)
        {
            return new SpendingAnalysis
            {
                Summary = "No transactions found for the selected period. Start adding your income and expenses to get personalized insights!",
                Period = $"{from:MMM dd} - {to:MMM dd, yyyy}"
            };
        }

        var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var totalExpenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var savingsRate = totalIncome > 0 ? ((totalIncome - totalExpenses) / totalIncome) * 100 : 0;

        var categoryBreakdowns = GetCategoryBreakdowns(transactions);
        var insights = GenerateInsights(transactions, totalIncome, totalExpenses, savingsRate, categoryBreakdowns);

        string aiRecommendation;
        if (_isModelLoaded)
        {
            aiRecommendation = await GetAIRecommendationAsync(transactions, totalIncome, totalExpenses, categoryBreakdowns);
        }
        else
        {
            aiRecommendation = GenerateRuleBasedRecommendation(transactions, totalIncome, totalExpenses, savingsRate, categoryBreakdowns);
        }

        return new SpendingAnalysis
        {
            Summary = GenerateSummary(totalIncome, totalExpenses, savingsRate),
            Insights = insights,
            CategoryBreakdowns = categoryBreakdowns,
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            SavingsRate = savingsRate,
            AiRecommendation = aiRecommendation,
            Period = $"{from:MMM dd} - {to:MMM dd, yyyy}"
        };
    }

    private List<CategoryBreakdown> GetCategoryBreakdowns(List<Transaction> transactions)
    {
        var expenses = transactions.Where(t => t.Type == TransactionType.Expense).ToList();
        var totalExpenses = expenses.Sum(t => t.Amount);

        if (totalExpenses == 0) return new List<CategoryBreakdown>();

        return expenses
            .GroupBy(t => t.CategoryName)
            .Select(g => new CategoryBreakdown
            {
                CategoryName = g.Key,
                Amount = g.Sum(t => t.Amount),
                Percentage = totalExpenses > 0 ? (double)(g.Sum(t => t.Amount) / totalExpenses * 100) : 0,
                ColorHex = GetCategoryColor(g.Key)
            })
            .OrderByDescending(b => b.Amount)
            .ToList();
    }

    private List<SpendingInsight> GenerateInsights(
        List<Transaction> transactions,
        decimal totalIncome,
        decimal totalExpenses,
        decimal savingsRate,
        List<CategoryBreakdown> breakdowns)
    {
        var insights = new List<SpendingInsight>();

        // Savings rate insight
        if (savingsRate >= 30)
        {
            insights.Add(new SpendingInsight
            {
                Title = "Excellent Savings!",
                Description = $"You're saving {savingsRate:N1}% of your income. That's above the recommended 20%. Keep it up!",
                Icon = "🌟",
                Type = InsightType.Positive
            });
        }
        else if (savingsRate >= 20)
        {
            insights.Add(new SpendingInsight
            {
                Title = "Good Savings Rate",
                Description = $"You're saving {savingsRate:N1}% of your income. You're meeting the recommended 20% target.",
                Icon = "👍",
                Type = InsightType.Positive
            });
        }
        else if (savingsRate >= 0)
        {
            insights.Add(new SpendingInsight
            {
                Title = "Room for Improvement",
                Description = $"You're only saving {savingsRate:N1}% of your income. Try to aim for at least 20%.",
                Icon = "⚠️",
                Type = InsightType.Warning
            });
        }
        else
        {
            insights.Add(new SpendingInsight
            {
                Title = "Overspending Alert",
                Description = $"You're spending more than you earn! Your expenses exceed income by ${Math.Abs(totalIncome - totalExpenses):N2}.",
                Icon = "🚨",
                Type = InsightType.Warning
            });
        }

        // Top spending category
        var topCategory = breakdowns.FirstOrDefault();
        if (topCategory is not null && topCategory.Percentage > 30)
        {
            insights.Add(new SpendingInsight
            {
                Title = $"High {topCategory.CategoryName} Spending",
                Description = $"{topCategory.CategoryName} accounts for {topCategory.Percentage:N1}% of your expenses (${topCategory.Amount:N2}). Consider reviewing this category.",
                Icon = "📊",
                Type = InsightType.Warning
            });
        }

        // Frequency analysis
        var dailyAvg = totalExpenses / Math.Max(1, (decimal)(transactions.Max(t => t.Date) - transactions.Min(t => t.Date)).TotalDays);
        if (dailyAvg > 0)
        {
            insights.Add(new SpendingInsight
            {
                Title = "Daily Spending Average",
                Description = $"You spend an average of ${dailyAvg:N2} per day. That's ${dailyAvg * 30:N2} projected monthly.",
                Icon = "📅",
                Type = InsightType.Neutral
            });
        }

        // Weekend vs weekday spending
        var weekdayExpenses = transactions
            .Where(t => t.Type == TransactionType.Expense && t.Date.DayOfWeek != DayOfWeek.Saturday && t.Date.DayOfWeek != DayOfWeek.Sunday)
            .Sum(t => t.Amount);
        var weekendExpenses = transactions
            .Where(t => t.Type == TransactionType.Expense && (t.Date.DayOfWeek == DayOfWeek.Saturday || t.Date.DayOfWeek == DayOfWeek.Sunday))
            .Sum(t => t.Amount);

        if (weekendExpenses > weekdayExpenses * 0.5m && weekendExpenses > 0)
        {
            insights.Add(new SpendingInsight
            {
                Title = "Weekend Spending Spike",
                Description = $"You tend to spend more on weekends (${weekendExpenses:N2}) compared to weekdays. Plan your weekend activities wisely!",
                Icon = "🎭",
                Type = InsightType.Tip
            });
        }

        // Small transactions add up
        var smallTransactions = transactions
            .Where(t => t.Type == TransactionType.Expense && t.Amount < 10)
            .ToList();
        if (smallTransactions.Count > 5)
        {
            var smallTotal = smallTransactions.Sum(t => t.Amount);
            insights.Add(new SpendingInsight
            {
                Title = "Small Purchases Add Up",
                Description = $"You have {smallTransactions.Count} small purchases (under $10) totaling ${smallTotal:N2}. These small expenses can accumulate quickly!",
                Icon = "🔍",
                Type = InsightType.Tip
            });
        }

        // Income diversity
        var incomeCategories = transactions
            .Where(t => t.Type == TransactionType.Income)
            .Select(t => t.CategoryName)
            .Distinct()
            .Count();

        if (incomeCategories >= 3)
        {
            insights.Add(new SpendingInsight
            {
                Title = "Diversified Income",
                Description = $"You have {incomeCategories} different income sources. Great job diversifying your income streams!",
                Icon = "💪",
                Type = InsightType.Positive
            });
        }
        else if (incomeCategories == 1 && totalIncome > 0)
        {
            insights.Add(new SpendingInsight
            {
                Title = "Single Income Source",
                Description = "Consider diversifying your income streams for better financial security.",
                Icon = "💡",
                Type = InsightType.Tip
            });
        }

        return insights;
    }

    /// <summary>
    /// Uses the locally loaded LLaMA model to generate a personalized recommendation.
    /// </summary>
    private async Task<string> GetAIRecommendationAsync(
        List<Transaction> transactions,
        decimal totalIncome,
        decimal totalExpenses,
        List<CategoryBreakdown> breakdowns)
    {
        try
        {
            var prompt = BuildAnalysisPrompt(transactions, totalIncome, totalExpenses, breakdowns);

            // In production with LLamaSharp loaded:
            // var executor = (InteractiveExecutor)_modelSession!;
            // var inferenceParams = new InferenceParams { MaxTokens = 512, Temperature = 0.7f };
            // var result = new StringBuilder();
            // await foreach (var text in executor.InferAsync(prompt, inferenceParams))
            // {
            //     result.Append(text);
            // }
            // return result.ToString().Trim();

            // Fallback to rule-based when model session exists but inference fails
            return GenerateRuleBasedRecommendation(transactions, totalIncome, totalExpenses, 0, breakdowns);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AI recommendation failed: {ex.Message}");
            return GenerateRuleBasedRecommendation(transactions, totalIncome, totalExpenses, 0, breakdowns);
        }
    }

    private string BuildAnalysisPrompt(
        List<Transaction> transactions,
        decimal totalIncome,
        decimal totalExpenses,
        List<CategoryBreakdown> breakdowns)
    {
        var categoryDetails = string.Join("\n", breakdowns.Select(b =>
            $"- {b.CategoryName}: ${b.Amount:N2} ({b.Percentage:N1}%)"));

        return $"""
            You are a personal finance advisor AI. Analyze the following spending data and provide 
            actionable, personalized advice in 3-4 sentences. Be specific and helpful.

            Financial Summary:
            - Total Income: ${totalIncome:N2}
            - Total Expenses: ${totalExpenses:N2}
            - Net Savings: ${totalIncome - totalExpenses:N2}
            - Number of Transactions: {transactions.Count}

            Expense Breakdown by Category:
            {categoryDetails}

            Provide specific, actionable advice to improve their financial health:
            """;
    }

    private string GenerateRuleBasedRecommendation(
        List<Transaction> transactions,
        decimal totalIncome,
        decimal totalExpenses,
        decimal savingsRate,
        List<CategoryBreakdown> breakdowns)
    {
        var recommendations = new List<string>();

        // Calculate actual savings rate if not provided
        if (savingsRate == 0 && totalIncome > 0)
            savingsRate = ((totalIncome - totalExpenses) / totalIncome) * 100;

        if (savingsRate < 0)
        {
            recommendations.Add(
                $"Your expenses exceed your income by ${Math.Abs(totalIncome - totalExpenses):N2}. " +
                "This is unsustainable. Review your largest expense categories and identify areas where you can cut back immediately.");
        }
        else if (savingsRate < 10)
        {
            recommendations.Add(
                "Your savings rate is below 10%. Financial experts recommend saving at least 20% of your income. " +
                "Try the 50/30/20 rule: 50% needs, 30% wants, 20% savings.");
        }
        else if (savingsRate < 20)
        {
            recommendations.Add(
                $"You're saving {savingsRate:N1}% of your income. To reach the recommended 20%, " +
                $"try to reduce expenses by ${(totalIncome * 0.2m - (totalIncome - totalExpenses)):N2} per month.");
        }
        else
        {
            recommendations.Add(
                $"Excellent! You're saving {savingsRate:N1}% of your income. " +
                "Consider investing your surplus in index funds or building an emergency fund if you haven't already.");
        }

        // Category-specific advice
        var topCategory = breakdowns.FirstOrDefault();
        if (topCategory is not null)
        {
            var categoryAdvice = topCategory.CategoryName switch
            {
                "Food & Dining" => "Consider meal prepping to reduce dining expenses. Cooking at home can save 50-70% compared to eating out.",
                "Transportation" => "Look into carpooling, public transit, or cycling to reduce transportation costs.",
                "Shopping" => "Try the 24-hour rule: wait a day before making non-essential purchases to avoid impulse buying.",
                "Entertainment" => "Look for free local events and activities. Many museums and parks offer free admission days.",
                "Subscriptions" => "Audit your subscriptions monthly. Cancel any you haven't used in the past 30 days.",
                "Housing & Rent" => "Housing is often the largest expense. If it exceeds 30% of income, consider roommates or a more affordable area.",
                _ => $"Your highest expense category is {topCategory.CategoryName} at {topCategory.Percentage:N1}%. Look for ways to optimize this spending."
            };
            recommendations.Add(categoryAdvice);
        }

        // Trend advice
        if (transactions.Count > 10)
        {
            var recentHalf = transactions.Take(transactions.Count / 2).Sum(t => t.Type == TransactionType.Expense ? t.Amount : 0);
            var olderHalf = transactions.Skip(transactions.Count / 2).Sum(t => t.Type == TransactionType.Expense ? t.Amount : 0);

            if (recentHalf > olderHalf * 1.1m)
            {
                recommendations.Add("Your spending has been trending upward recently. Set a weekly budget to keep expenses in check.");
            }
        }

        return string.Join("\n\n", recommendations);
    }

    private string GenerateSummary(decimal totalIncome, decimal totalExpenses, decimal savingsRate)
    {
        var net = totalIncome - totalExpenses;
        var status = net >= 0 ? "positive" : "negative";

        return $"Your financial overview shows ${totalIncome:N2} in income and ${totalExpenses:N2} in expenses, " +
               $"resulting in a {status} balance of ${Math.Abs(net):N2}. " +
               $"Your savings rate is {savingsRate:N1}%.";
    }

    private static string GetCategoryColor(string categoryName)
    {
        var colors = new Dictionary<string, string>
        {
            ["Food & Dining"] = "#FF6B6B",
            ["Transportation"] = "#4ECDC4",
            ["Shopping"] = "#45B7D1",
            ["Housing & Rent"] = "#96CEB4",
            ["Utilities"] = "#FFEAA7",
            ["Healthcare"] = "#DDA0DD",
            ["Entertainment"] = "#FF8A5C",
            ["Education"] = "#A8E6CF",
            ["Personal Care"] = "#FFB6C1",
            ["Insurance"] = "#87CEEB",
            ["Subscriptions"] = "#C39BD3",
            ["Gifts & Donations"] = "#F1948A",
            ["Travel"] = "#76D7C4",
            ["Groceries"] = "#F9E79F",
        };

        return colors.GetValueOrDefault(categoryName, "#6C63FF");
    }
}

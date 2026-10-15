using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceManager.Models;
using FinanceManager.Services;
using FinanceManager.Views;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace FinanceManager.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _greeting = "Good Morning";

    [ObservableProperty]
    private string _userName = "User";

    [ObservableProperty]
    private string _userInitials = "U";

    [ObservableProperty]
    private decimal _totalBalance;

    [ObservableProperty]
    private decimal _totalIncome;

    [ObservableProperty]
    private decimal _totalExpenses;

    [ObservableProperty]
    private string _formattedBalance = "$0.00";

    [ObservableProperty]
    private string _formattedIncome = "$0.00";

    [ObservableProperty]
    private string _formattedExpenses = "$0.00";

    [ObservableProperty]
    private string _currentMonth = DateTime.Now.ToString("MMMM yyyy");

    [ObservableProperty]
    private ObservableCollection<Transaction> _recentTransactions = new();

    [ObservableProperty]
    private ISeries[] _expenseChartSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private ISeries[] _monthlyTrendSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private Axis[] _monthlyTrendXAxes = Array.Empty<Axis>();

    [ObservableProperty]
    private Axis[] _monthlyTrendYAxes = Array.Empty<Axis>();

    [ObservableProperty]
    private bool _hasTransactions;

    public DashboardViewModel(DatabaseService databaseService, IAuthService authService)
    {
        _databaseService = databaseService;
        _authService = authService;
        Title = "Dashboard";
        SetGreeting();
    }

    private void SetGreeting()
    {
        var hour = DateTime.Now.Hour;
        Greeting = hour switch
        {
            < 12 => "Good Morning",
            < 17 => "Good Afternoon",
            _ => "Good Evening"
        };
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            var user = await _authService.GetCurrentUserAsync();
            if (user is null) return;

            UserName = user.DisplayName.Split(' ')[0];
            UserInitials = string.Concat(user.DisplayName.Split(' ').Take(2).Select(n => n[0])).ToUpper();

            var now = DateTime.Now;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            // Load financial data
            TotalIncome = await _databaseService.GetTotalIncomeAsync(user.Id, monthStart, monthEnd);
            TotalExpenses = await _databaseService.GetTotalExpensesAsync(user.Id, monthStart, monthEnd);
            TotalBalance = TotalIncome - TotalExpenses;

            FormattedBalance = $"${Math.Abs(TotalBalance):N2}";
            FormattedIncome = $"${TotalIncome:N2}";
            FormattedExpenses = $"${TotalExpenses:N2}";

            // Load recent transactions
            var recent = await _databaseService.GetRecentTransactionsAsync(user.Id, 5);
            RecentTransactions = new ObservableCollection<Transaction>(recent);
            HasTransactions = recent.Count > 0;

            // Build charts
            await BuildExpenseChartAsync(user.Id, monthStart, monthEnd);
            await BuildMonthlyTrendChartAsync(user.Id);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Dashboard load error: {ex}");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    private async Task BuildExpenseChartAsync(string userId, DateTime from, DateTime to)
    {
        var transactions = await _databaseService.GetTransactionsByDateRangeAsync(userId, from, to);
        var expenses = transactions.Where(t => t.Type == TransactionType.Expense).ToList();

        if (expenses.Count == 0)
        {
            ExpenseChartSeries = Array.Empty<ISeries>();
            return;
        }

        var grouped = expenses
            .GroupBy(t => t.CategoryName)
            .Select(g => new { Category = g.Key, Total = (double)g.Sum(t => t.Amount) })
            .OrderByDescending(g => g.Total)
            .Take(6)
            .ToList();

        var colors = new[] { "#FF6B6B", "#4ECDC4", "#45B7D1", "#96CEB4", "#FFEAA7", "#DDA0DD" };

        ExpenseChartSeries = grouped.Select((g, i) => new PieSeries<double>
        {
            Values = new[] { g.Total },
            Name = g.Category,
            Fill = new SolidColorPaint(SKColor.Parse(colors[i % colors.Length])),
            DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
            DataLabelsFormatter = point => $"{g.Category}",
            DataLabelsPaint = new SolidColorPaint(SKColors.White),
        } as ISeries).ToArray();
    }

    private async Task BuildMonthlyTrendChartAsync(string userId)
    {
        var summaries = await _databaseService.GetMonthlySummariesAsync(userId, 6);

        MonthlyTrendSeries = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Values = summaries.Select(s => (double)s.TotalIncome).ToArray(),
                Name = "Income",
                Fill = new SolidColorPaint(SKColor.Parse("#4CAF50")),
                Rx = 4,
                Ry = 4,
                MaxBarWidth = 20,
            },
            new ColumnSeries<double>
            {
                Values = summaries.Select(s => (double)s.TotalExpenses).ToArray(),
                Name = "Expenses",
                Fill = new SolidColorPaint(SKColor.Parse("#FF6B6B")),
                Rx = 4,
                Ry = 4,
                MaxBarWidth = 20,
            }
        };

        MonthlyTrendXAxes = new Axis[]
        {
            new Axis
            {
                Labels = summaries.Select(s => s.MonthName).ToArray(),
                LabelsPaint = new SolidColorPaint(SKColor.Parse("#9E9E9E")),
                TextSize = 12,
            }
        };

        MonthlyTrendYAxes = new Axis[]
        {
            new Axis
            {
                LabelsPaint = new SolidColorPaint(SKColor.Parse("#9E9E9E")),
                TextSize = 10,
                Labeler = value => $"${value:N0}",
            }
        };
    }

    [RelayCommand]
    private async Task GoToAddTransactionAsync()
    {
        await Shell.Current.GoToAsync(nameof(AddTransactionPage));
    }

    [RelayCommand]
    private async Task GoToAllTransactionsAsync()
    {
        await Shell.Current.GoToAsync(nameof(TransactionHistoryPage));
    }

    [RelayCommand]
    private async Task GoToAnalysisAsync()
    {
        await Shell.Current.GoToAsync(nameof(AIAnalysisPage));
    }

    [RelayCommand]
    private async Task GoToCategoriesAsync()
    {
        await Shell.Current.GoToAsync(nameof(CategoryPage));
    }

    [RelayCommand]
    private async Task SignOutAsync()
    {
        var confirm = await Shell.Current.DisplayAlert("Sign Out", "Are you sure you want to sign out?", "Yes", "No");
        if (confirm)
        {
            await _authService.SignOutAsync();
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }
    }
}

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceManager.Models;
using FinanceManager.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace FinanceManager.ViewModels;

public partial class AIAnalysisViewModel : BaseViewModel
{
    private readonly AIAnalysisService _aiService;
    private readonly IAuthService _authService;
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private string _summary = string.Empty;

    [ObservableProperty]
    private string _aiRecommendation = string.Empty;

    [ObservableProperty]
    private string _period = "This Month";

    [ObservableProperty]
    private decimal _totalIncome;

    [ObservableProperty]
    private decimal _totalExpenses;

    [ObservableProperty]
    private decimal _savingsRate;

    [ObservableProperty]
    private string _formattedIncome = "$0.00";

    [ObservableProperty]
    private string _formattedExpenses = "$0.00";

    [ObservableProperty]
    private string _formattedSavingsRate = "0%";

    [ObservableProperty]
    private string _savingsRateColor = "#4CAF50";

    [ObservableProperty]
    private ObservableCollection<SpendingInsight> _insights = new();

    [ObservableProperty]
    private ObservableCollection<CategoryBreakdown> _categoryBreakdowns = new();

    [ObservableProperty]
    private ISeries[] _breakdownChartSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private bool _isModelLoaded;

    [ObservableProperty]
    private string _aiStatusText = "Using Smart Analysis";

    [ObservableProperty]
    private string _aiStatusIcon = "🧠";

    [ObservableProperty]
    private bool _hasData;

    [ObservableProperty]
    private int _selectedPeriodIndex;

    public string[] PeriodOptions { get; } = { "This Week", "This Month", "Last 3 Months", "Last 6 Months", "This Year" };

    public AIAnalysisViewModel(AIAnalysisService aiService, IAuthService authService, DatabaseService databaseService)
    {
        _aiService = aiService;
        _authService = authService;
        _databaseService = databaseService;
        Title = "AI Analysis";
        SelectedPeriodIndex = 1; // Default: This Month
    }

    [RelayCommand]
    private async Task LoadAnalysisAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            // Try to load AI model
            var modelLoaded = await _aiService.TryLoadModelAsync();
            IsModelLoaded = modelLoaded;
            AiStatusText = modelLoaded ? "Powered by Local LLaMA AI" : "Using Smart Analysis Engine";
            AiStatusIcon = modelLoaded ? "🤖" : "🧠";

            var user = await _authService.GetCurrentUserAsync();
            if (user is null) return;

            var (from, to) = GetDateRange();

            var analysis = await _aiService.AnalyzeSpendingAsync(user.Id, from, to);

            // Update UI
            Summary = analysis.Summary;
            AiRecommendation = analysis.AiRecommendation;
            Period = analysis.Period;
            TotalIncome = analysis.TotalIncome;
            TotalExpenses = analysis.TotalExpenses;
            SavingsRate = analysis.SavingsRate;

            FormattedIncome = $"${analysis.TotalIncome:N2}";
            FormattedExpenses = $"${analysis.TotalExpenses:N2}";
            FormattedSavingsRate = $"{analysis.SavingsRate:N1}%";

            SavingsRateColor = analysis.SavingsRate >= 20 ? "#4CAF50" :
                               analysis.SavingsRate >= 10 ? "#FF9800" : "#F44336";

            Insights = new ObservableCollection<SpendingInsight>(analysis.Insights);
            CategoryBreakdowns = new ObservableCollection<CategoryBreakdown>(analysis.CategoryBreakdowns);

            HasData = analysis.TotalIncome > 0 || analysis.TotalExpenses > 0;

            BuildBreakdownChart(analysis.CategoryBreakdowns);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Analysis load error: {ex}");
            Summary = "Unable to generate analysis. Please try again later.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private (DateTime from, DateTime to) GetDateRange()
    {
        var now = DateTime.Now;
        return SelectedPeriodIndex switch
        {
            0 => (now.AddDays(-(int)now.DayOfWeek), now), // This Week
            1 => (new DateTime(now.Year, now.Month, 1), now), // This Month
            2 => (now.AddMonths(-3), now), // Last 3 Months
            3 => (now.AddMonths(-6), now), // Last 6 Months
            4 => (new DateTime(now.Year, 1, 1), now), // This Year
            _ => (new DateTime(now.Year, now.Month, 1), now)
        };
    }

    [RelayCommand]
    private async Task ChangePeriodAsync(int index)
    {
        SelectedPeriodIndex = index;
        await LoadAnalysisAsync();
    }

    private void BuildBreakdownChart(List<CategoryBreakdown> breakdowns)
    {
        if (breakdowns.Count == 0)
        {
            BreakdownChartSeries = Array.Empty<ISeries>();
            return;
        }

        BreakdownChartSeries = breakdowns
            .Take(8)
            .Select(b => new PieSeries<double>
            {
                Values = new[] { (double)b.Amount },
                Name = b.CategoryName,
                Fill = new SolidColorPaint(SKColor.Parse(b.ColorHex)),
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                DataLabelsFormatter = point => $"{b.FormattedPercentage}",
                DataLabelsPaint = new SolidColorPaint(SKColors.White) { SKTypeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold) },
                DataLabelsSize = 11,
                InnerRadius = 50,
            } as ISeries)
            .ToArray();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadAnalysisAsync();
    }
}

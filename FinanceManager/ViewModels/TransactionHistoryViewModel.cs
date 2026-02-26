using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceManager.Models;
using FinanceManager.Services;

namespace FinanceManager.ViewModels;

public partial class TransactionHistoryViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private ObservableCollection<TransactionGroup> _transactionGroups = new();

    [ObservableProperty]
    private string _filterText = "All";

    [ObservableProperty]
    private bool _showAllFilter = true;

    [ObservableProperty]
    private bool _showIncomeFilter;

    [ObservableProperty]
    private bool _showExpenseFilter;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _hasTransactions;

    [ObservableProperty]
    private string _totalLabel = "$0.00";

    private TransactionType? _currentFilter;

    public TransactionHistoryViewModel(DatabaseService databaseService, IAuthService authService)
    {
        _databaseService = databaseService;
        _authService = authService;
        Title = "Transaction History";
    }

    [RelayCommand]
    private async Task LoadTransactionsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            var user = await _authService.GetCurrentUserAsync();
            if (user is null) return;

            List<Transaction> transactions;
            if (_currentFilter.HasValue)
            {
                transactions = await _databaseService.GetTransactionsByTypeAsync(user.Id, _currentFilter.Value);
            }
            else
            {
                transactions = await _databaseService.GetTransactionsAsync(user.Id);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var query = SearchQuery.ToLower();
                transactions = transactions
                    .Where(t => t.Description.ToLower().Contains(query) ||
                                t.CategoryName.ToLower().Contains(query) ||
                                t.Notes.ToLower().Contains(query))
                    .ToList();
            }

            // Group by date
            var groups = transactions
                .GroupBy(t => t.Date.Date)
                .OrderByDescending(g => g.Key)
                .Select(g => new TransactionGroup(
                    g.Key.ToString("MMMM dd, yyyy"),
                    g.Key == DateTime.Today ? "Today" :
                    g.Key == DateTime.Today.AddDays(-1) ? "Yesterday" :
                    g.Key.ToString("MMMM dd, yyyy"),
                    new ObservableCollection<Transaction>(g.OrderByDescending(t => t.CreatedAt))))
                .ToList();

            TransactionGroups = new ObservableCollection<TransactionGroup>(groups);
            HasTransactions = groups.Count > 0;

            // Calculate total
            var total = transactions.Sum(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount);
            TotalLabel = total >= 0 ? $"+${total:N2}" : $"-${Math.Abs(total):N2}";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Load transactions error: {ex}");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task FilterAllAsync()
    {
        _currentFilter = null;
        FilterText = "All";
        ShowAllFilter = true;
        ShowIncomeFilter = false;
        ShowExpenseFilter = false;
        await LoadTransactionsAsync();
    }

    [RelayCommand]
    private async Task FilterIncomeAsync()
    {
        _currentFilter = TransactionType.Income;
        FilterText = "Income";
        ShowAllFilter = false;
        ShowIncomeFilter = true;
        ShowExpenseFilter = false;
        await LoadTransactionsAsync();
    }

    [RelayCommand]
    private async Task FilterExpenseAsync()
    {
        _currentFilter = TransactionType.Expense;
        FilterText = "Expenses";
        ShowAllFilter = false;
        ShowIncomeFilter = false;
        ShowExpenseFilter = true;
        await LoadTransactionsAsync();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadTransactionsAsync();
    }

    [RelayCommand]
    private async Task DeleteTransactionAsync(Transaction transaction)
    {
        var confirm = await Shell.Current.DisplayAlert("Delete Transaction",
            $"Delete {transaction.Description} (${transaction.Amount:N2})?", "Delete", "Cancel");

        if (confirm)
        {
            await _databaseService.DeleteTransactionAsync(transaction);
            await LoadTransactionsAsync();
        }
    }
}

public class TransactionGroup : ObservableCollection<Transaction>
{
    public string Date { get; }
    public string DisplayDate { get; }

    public TransactionGroup(string date, string displayDate, ObservableCollection<Transaction> transactions) : base(transactions)
    {
        Date = date;
        DisplayDate = displayDate;
    }
}

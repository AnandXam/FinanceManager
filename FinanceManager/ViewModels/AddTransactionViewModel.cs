using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceManager.Models;
using FinanceManager.Services;

namespace FinanceManager.ViewModels;

public partial class AddTransactionViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private TransactionType _selectedType = TransactionType.Expense;

    [ObservableProperty]
    private string _amount = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Today;

    [ObservableProperty]
    private Category? _selectedCategory;

    [ObservableProperty]
    private ObservableCollection<Category> _categories = new();

    [ObservableProperty]
    private bool _isExpenseSelected = true;

    [ObservableProperty]
    private bool _isIncomeSelected;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    public AddTransactionViewModel(DatabaseService databaseService, IAuthService authService)
    {
        _databaseService = databaseService;
        _authService = authService;
        Title = "Add Transaction";
    }

    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user is null) return;

        var categories = await _databaseService.GetCategoriesByTypeAsync(user.Id, SelectedType);
        Categories = new ObservableCollection<Category>(categories);
    }

    [RelayCommand]
    private async Task SelectExpenseAsync()
    {
        SelectedType = TransactionType.Expense;
        IsExpenseSelected = true;
        IsIncomeSelected = false;
        SelectedCategory = null;
        await LoadCategoriesAsync();
    }

    [RelayCommand]
    private async Task SelectIncomeAsync()
    {
        SelectedType = TransactionType.Income;
        IsExpenseSelected = false;
        IsIncomeSelected = true;
        SelectedCategory = null;
        await LoadCategoriesAsync();
    }

    [RelayCommand]
    private async Task SaveTransactionAsync()
    {
        if (IsBusy) return;

        // Validation
        HasError = false;
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Amount) || !decimal.TryParse(Amount, out var amountValue) || amountValue <= 0)
        {
            HasError = true;
            ErrorMessage = "Please enter a valid amount greater than 0.";
            return;
        }

        if (SelectedCategory is null)
        {
            HasError = true;
            ErrorMessage = "Please select a category.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            HasError = true;
            ErrorMessage = "Please enter a description.";
            return;
        }

        try
        {
            IsBusy = true;

            var user = await _authService.GetCurrentUserAsync();
            if (user is null) return;

            var transaction = new Transaction
            {
                UserId = user.Id,
                Type = SelectedType,
                Amount = amountValue,
                Description = Description.Trim(),
                CategoryId = SelectedCategory.Id,
                CategoryName = SelectedCategory.Name,
                Date = SelectedDate,
                Notes = Notes?.Trim() ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            await _databaseService.SaveTransactionAsync(transaction);

            // Show success and navigate back
            await Shell.Current.DisplayAlert("Success",
                $"{(SelectedType == TransactionType.Income ? "Income" : "Expense")} of ${amountValue:N2} added successfully!",
                "OK");

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = "Failed to save transaction. Please try again.";
            System.Diagnostics.Debug.WriteLine($"Save transaction error: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}

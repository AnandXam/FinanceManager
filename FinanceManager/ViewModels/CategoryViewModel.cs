using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceManager.Models;
using FinanceManager.Services;

namespace FinanceManager.ViewModels;

public partial class CategoryViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private ObservableCollection<Category> _expenseCategories = new();

    [ObservableProperty]
    private ObservableCollection<Category> _incomeCategories = new();

    [ObservableProperty]
    private bool _isExpenseTabSelected = true;

    [ObservableProperty]
    private string _newCategoryName = string.Empty;

    [ObservableProperty]
    private string _newCategoryIcon = "📌";

    [ObservableProperty]
    private string _newCategoryColor = "#6C63FF";

    [ObservableProperty]
    private bool _isAddingCategory;

    [ObservableProperty]
    private ObservableCollection<SuggestedCategory> _suggestedCategories = new();

    public CategoryViewModel(DatabaseService databaseService, IAuthService authService)
    {
        _databaseService = databaseService;
        _authService = authService;
        Title = "Categories";
    }

    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            var user = await _authService.GetCurrentUserAsync();
            if (user is null) return;

            var expenseCategories = await _databaseService.GetCategoriesByTypeAsync(user.Id, TransactionType.Expense);
            var incomeCategories = await _databaseService.GetCategoriesByTypeAsync(user.Id, TransactionType.Income);

            ExpenseCategories = new ObservableCollection<Category>(expenseCategories);
            IncomeCategories = new ObservableCollection<Category>(incomeCategories);

            LoadSuggestedCategories();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Load categories error: {ex}");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    private void LoadSuggestedCategories()
    {
        var existingNames = ExpenseCategories.Select(c => c.Name)
            .Concat(IncomeCategories.Select(c => c.Name))
            .ToHashSet();

        var suggestions = new List<SuggestedCategory>();

        if (IsExpenseTabSelected)
        {
            foreach (var (name, icon, color) in CategoryService.GetSuggestedExpenseCategories())
            {
                if (!existingNames.Contains(name))
                {
                    suggestions.Add(new SuggestedCategory { Name = name, Icon = icon, ColorHex = color, Type = TransactionType.Expense });
                }
            }
        }
        else
        {
            foreach (var (name, icon, color) in CategoryService.GetSuggestedIncomeCategories())
            {
                if (!existingNames.Contains(name))
                {
                    suggestions.Add(new SuggestedCategory { Name = name, Icon = icon, ColorHex = color, Type = TransactionType.Income });
                }
            }
        }

        SuggestedCategories = new ObservableCollection<SuggestedCategory>(suggestions);
    }

    [RelayCommand]
    private void SelectExpenseTab()
    {
        IsExpenseTabSelected = true;
        LoadSuggestedCategories();
    }

    [RelayCommand]
    private void SelectIncomeTab()
    {
        IsExpenseTabSelected = false;
        LoadSuggestedCategories();
    }

    [RelayCommand]
    private void ToggleAddCategory()
    {
        IsAddingCategory = !IsAddingCategory;
        if (!IsAddingCategory)
        {
            NewCategoryName = string.Empty;
            NewCategoryIcon = "📌";
            NewCategoryColor = "#6C63FF";
        }
    }

    [RelayCommand]
    private async Task AddCategoryAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCategoryName))
        {
            await Shell.Current.DisplayAlert("Error", "Please enter a category name.", "OK");
            return;
        }

        var user = await _authService.GetCurrentUserAsync();
        if (user is null) return;

        var category = new Category
        {
            UserId = user.Id,
            Name = NewCategoryName.Trim(),
            Icon = NewCategoryIcon,
            ColorHex = NewCategoryColor,
            Type = IsExpenseTabSelected ? TransactionType.Expense : TransactionType.Income,
            IsDefault = false
        };

        await _databaseService.SaveCategoryAsync(category);

        NewCategoryName = string.Empty;
        NewCategoryIcon = "📌";
        IsAddingCategory = false;

        await LoadCategoriesAsync();
    }

    [RelayCommand]
    private async Task AddSuggestedCategoryAsync(SuggestedCategory suggested)
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user is null) return;

        var category = new Category
        {
            UserId = user.Id,
            Name = suggested.Name,
            Icon = suggested.Icon,
            ColorHex = suggested.ColorHex,
            Type = suggested.Type,
            IsDefault = false
        };

        await _databaseService.SaveCategoryAsync(category);
        await LoadCategoriesAsync();
    }

    [RelayCommand]
    private async Task DeleteCategoryAsync(Category category)
    {
        if (category.IsDefault)
        {
            await Shell.Current.DisplayAlert("Cannot Delete", "Default categories cannot be deleted.", "OK");
            return;
        }

        var confirm = await Shell.Current.DisplayAlert("Delete Category",
            $"Are you sure you want to delete '{category.Name}'?", "Delete", "Cancel");

        if (confirm)
        {
            await _databaseService.DeleteCategoryAsync(category);
            await LoadCategoriesAsync();
        }
    }
}

public class SuggestedCategory
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
}

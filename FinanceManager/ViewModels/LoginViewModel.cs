using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceManager.Services;
using FinanceManager.Views;

namespace FinanceManager.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly CategoryService _categoryService;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    public LoginViewModel(IAuthService authService, CategoryService categoryService)
    {
        _authService = authService;
        _categoryService = categoryService;
        Title = "Welcome";
    }

    [RelayCommand]
    private async Task SignInWithGoogleAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;

            var user = await _authService.SignInWithGoogleAsync();
            if (user is not null)
            {
                // Seed default categories for new users
                await _categoryService.SeedDefaultCategoriesAsync(user.Id);

                // Navigate to Dashboard
                await Shell.Current.GoToAsync($"//{nameof(DashboardPage)}");
            }
        }
        catch (AuthenticationException ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = "An unexpected error occurred. Please try again.";
            System.Diagnostics.Debug.WriteLine($"Login error: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SignInWithAppleAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;

            var user = await _authService.SignInWithAppleAsync();
            if (user is not null)
            {
                await _categoryService.SeedDefaultCategoriesAsync(user.Id);
                await Shell.Current.GoToAsync($"//{nameof(DashboardPage)}");
            }
        }
        catch (AuthenticationException ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = "An unexpected error occurred. Please try again.";
            System.Diagnostics.Debug.WriteLine($"Login error: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}

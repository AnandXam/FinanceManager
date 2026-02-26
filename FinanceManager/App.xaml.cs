using FinanceManager.Services;
using FinanceManager.Views;

namespace FinanceManager;

public partial class App : Application
{
    private readonly IAuthService _authService;

    public App(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        MainPage = new AppShell();
    }

    protected override async void OnStart()
    {
        base.OnStart();

        // Check if user is already authenticated
        var user = await _authService.GetCurrentUserAsync();
        if (user is not null)
        {
            // Navigate directly to dashboard
            await Shell.Current.GoToAsync($"//{nameof(DashboardPage)}");
        }
    }
}

using FinanceManager.Views;

namespace FinanceManager;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes for navigation
        Routing.RegisterRoute(nameof(AddTransactionPage), typeof(AddTransactionPage));
        Routing.RegisterRoute(nameof(CategoryPage), typeof(CategoryPage));
        Routing.RegisterRoute(nameof(TransactionHistoryPage), typeof(TransactionHistoryPage));
        Routing.RegisterRoute(nameof(AIAnalysisPage), typeof(AIAnalysisPage));
    }
}

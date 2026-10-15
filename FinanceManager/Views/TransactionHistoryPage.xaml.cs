using FinanceManager.ViewModels;

namespace FinanceManager.Views;

public partial class TransactionHistoryPage : ContentPage
{
    private readonly TransactionHistoryViewModel _viewModel;

    public TransactionHistoryPage(TransactionHistoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadTransactionsCommand.ExecuteAsync(null);
    }
}

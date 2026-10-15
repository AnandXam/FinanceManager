using FinanceManager.ViewModels;

namespace FinanceManager.Views;

public partial class AIAnalysisPage : ContentPage
{
    private readonly AIAnalysisViewModel _viewModel;

    public AIAnalysisPage(AIAnalysisViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAnalysisCommand.ExecuteAsync(null);
    }
}

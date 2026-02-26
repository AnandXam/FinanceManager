using CommunityToolkit.Maui;
using FinanceManager.Services;
using FinanceManager.ViewModels;
using FinanceManager.Views;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace FinanceManager;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Configure LiveCharts theme for dark mode
        LiveCharts.Configure(config =>
            config
                .AddDarkTheme()
                .AddSkiaSharp()
        );

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // ─── Register Services ─────────────────────────────────
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<CategoryService>();
        builder.Services.AddSingleton<AIAnalysisService>();

        // ─── Register ViewModels ───────────────────────────────
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<AddTransactionViewModel>();
        builder.Services.AddTransient<CategoryViewModel>();
        builder.Services.AddTransient<TransactionHistoryViewModel>();
        builder.Services.AddTransient<AIAnalysisViewModel>();

        // ─── Register Pages ────────────────────────────────────
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<AddTransactionPage>();
        builder.Services.AddTransient<CategoryPage>();
        builder.Services.AddTransient<TransactionHistoryPage>();
        builder.Services.AddTransient<AIAnalysisPage>();

        return builder.Build();
    }
}

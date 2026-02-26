# Finance Manager - .NET MAUI App

A cross-platform personal finance management application built with .NET MAUI, featuring Google & Apple SSO authentication, beautiful dark-themed dashboard with interactive charts, comprehensive category management, and AI-powered spending analysis using local LLaMA models.

## Features

### Authentication
- **Google Sign-In** - Seamless authentication via Google OAuth / Firebase Auth
- **Apple Sign-In** - Native Apple SSO on iOS/macOS, web-based on Android
- **Persistent sessions** - Stay logged in across app restarts

### Dashboard
- **Real-time financial overview** - Total balance, income, and expenses at a glance
- **Interactive pie charts** - Expense breakdown by category using LiveChartsCore
- **Monthly trend charts** - 6-month income vs. expense comparison bar chart
- **Recent transactions** - Quick view of your latest 5 transactions
- **Quick action buttons** - One-tap access to add transactions, categories, history, and AI insights

### Transaction Management
- **Add income & expenses** - Clean, intuitive form with amount, description, category, date, and notes
- **Category selection** - Visual grid picker with emoji icons
- **Transaction history** - Grouped by date with search and filter (All / Income / Expenses)
- **Swipe to delete** - Easy transaction removal

### Category Management
- **25 default categories** - Pre-seeded expense and income categories with emoji icons and colors
- **Custom categories** - Create your own with custom name, icon, and color
- **Suggested categories** - AI-suggested additional categories you might need
- **Swipe to delete** - Remove custom categories (defaults are protected)

### AI-Powered Analysis (Local LLaMA)
- **Local AI processing** - All analysis runs on-device using LLamaSharp (llama.cpp bindings)
- **Spending pattern analysis** - Identifies trends, anomalies, and patterns
- **Smart insights** - Savings rate evaluation, top spending alerts, daily averages, weekend spending spikes
- **Category breakdown** - Donut chart with percentage bars
- **Personalized recommendations** - Category-specific advice (e.g., meal prep for food, 24-hour rule for shopping)
- **Rule-based fallback** - Comprehensive analysis even without the AI model loaded
- **Period selection** - Analyze by week, month, 3 months, 6 months, or year

## Architecture

```
FinanceManager/
├── Models/
│   ├── Transaction.cs          # Income/expense data model
│   ├── Category.cs             # Category with icon, color, type
│   ├── UserProfile.cs          # User authentication profile
│   └── SpendingAnalysis.cs     # AI analysis results & insights
├── Services/
│   ├── DatabaseService.cs      # SQLite CRUD operations
│   ├── AuthService.cs          # Google & Apple SSO implementation
│   ├── IAuthService.cs         # Auth interface
│   ├── CategoryService.cs      # Default & suggested categories
│   └── AIAnalysisService.cs    # LLaMA integration & rule-based analysis
├── ViewModels/
│   ├── BaseViewModel.cs        # MVVM base with IsBusy, Title
│   ├── LoginViewModel.cs       # Authentication logic
│   ├── DashboardViewModel.cs   # Dashboard data & charts
│   ├── AddTransactionViewModel.cs
│   ├── CategoryViewModel.cs    # Category CRUD & suggestions
│   ├── TransactionHistoryViewModel.cs
│   └── AIAnalysisViewModel.cs  # AI analysis & period selection
├── Views/
│   ├── LoginPage.xaml          # Dark SSO login screen
│   ├── DashboardPage.xaml      # Main dashboard with charts
│   ├── AddTransactionPage.xaml # Transaction form
│   ├── CategoryPage.xaml       # Category management
│   ├── TransactionHistoryPage.xaml
│   └── AIAnalysisPage.xaml     # AI insights & recommendations
├── Converters/
│   └── ValueConverters.cs      # 12+ XAML value converters
├── Resources/
│   └── Styles/
│       ├── Colors.xaml         # Dark theme color palette
│       └── Styles.xaml         # Global styles
└── Platforms/
    ├── Android/
    ├── iOS/
    ├── MacCatalyst/
    └── Windows/
```

## Tech Stack

| Component | Technology |
|-----------|-----------|
| Framework | .NET 10 MAUI |
| Architecture | MVVM (CommunityToolkit.Mvvm) |
| Database | SQLite (sqlite-net-pcl) |
| Charts | LiveChartsCore.SkiaSharpView.Maui |
| AI Engine | LLamaSharp (llama.cpp C# bindings) |
| Auth | Firebase Auth (Google & Apple SSO) |
| UI Toolkit | CommunityToolkit.Maui |

## Getting Started

### Prerequisites
- .NET 10 SDK
- Visual Studio 2026+ with MAUI workload, or JetBrains Rider
- Android SDK (API 24+) for Android target
- Xcode 26+ (for iOS/macOS targets)

### Setup

1. Clone the repository
2. Open `FinanceManager.sln` in Visual Studio
3. Restore NuGet packages
4. Configure Firebase:
   - Create a Firebase project at https://console.firebase.google.com
   - Enable Google Sign-In and Apple Sign-In providers
   - Download `google-services.json` (Android) and `GoogleService-Info.plist` (iOS)
   - Place them in the respective platform folders

### Running
```bash
# Android
dotnet build -t:Run -f net10.0-android

# iOS (macOS only)
dotnet build -t:Run -f net10.0-ios

# Windows
dotnet build -t:Run -f net10.0-windows10.0.19041.0
```

### AI Model Setup (Optional)
To enable local LLaMA AI analysis:
1. Download a GGUF model file (e.g., Llama-2-7B-Chat or a smaller quantized model)
2. Place it at: `{AppDataDirectory}/models/llama-finance.gguf`
3. The app will automatically detect and use the model for enhanced analysis

Without the model, the app uses a comprehensive rule-based analysis engine.

## Design

The app features a **modern dark theme** with:
- Deep navy backgrounds (`#0F0F23`, `#1A1A3E`)
- Purple accent color (`#6C63FF`) for primary actions
- Green (`#4CAF50`) for income, Red (`#FF6B6B`) for expenses
- Rounded cards with subtle shadows
- Emoji-based category icons
- Gradient balance card
- Smooth animations and transitions

## License

MIT License

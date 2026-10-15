using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using System;

namespace FinanceManager;

class Program : global::Microsoft.Maui.MauiTizenApplication
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    static void Main(string[] args)
    {
        var app = new Program();
        app.Run(args);
    }
}

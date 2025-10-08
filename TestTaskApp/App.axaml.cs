using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveUI;
using Splat;
using System;
using System.Runtime.InteropServices.JavaScript;
using TestTaskApp.ViewModels;
using TestTaskApp.Views;

namespace TestTaskApp;

public partial class App : Application
{
    public static IHost Host { get; set; } = null!;
    public override void Initialize()
    {
        Console.Write("=================");
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Console.Write("=================");
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainVM = App.Host.Services.GetRequiredService<MainViewModel>();

            Locator.CurrentMutable.Register(() => new IndexView(), typeof(IViewFor<IndexViewModel>));
            Locator.CurrentMutable.Register(() => new RulesView(), typeof(IViewFor<RulesViewModel>));

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainVM
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}

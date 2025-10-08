using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using TestTaskApp.services;
using TestTaskApp.ViewModels;

namespace TestTaskApp.Desktop;

public class Program
{
    private static IHost? _host;

    [STAThread] 
    public static void Main(string[] args)
    {
        _host = CreateHostBuilder(args).Build();
        Console.WriteLine("started app");

        _host.StartAsync().GetAwaiter().GetResult();

        BuildAvaloniaApp()
            .AfterSetup(_ => App.Host = _host!)
            .StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);

        _host.StopAsync().GetAwaiter().GetResult();
        _host.Dispose();
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<ServiceActors>();
                services.AddSingleton<ServiceSensors>();
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<ServiceUserRules>();
                services.AddSingleton<ServiceRulesScheduler>();
                services.AddHostedService(provider => provider.GetRequiredService<ServiceRulesScheduler>());
                services.AddHostedService<ServiceSensorSimulation>();
            });

    public static T GetService<T>() where T : notnull =>
        _host!.Services.GetRequiredService<T>();
}

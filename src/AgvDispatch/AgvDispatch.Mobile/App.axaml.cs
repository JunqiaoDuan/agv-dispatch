using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AgvDispatch.Mobile.Services;
using AgvDispatch.Mobile.ViewModels;
using AgvDispatch.Mobile.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace AgvDispatch.Mobile;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // 配置依赖注入
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var authService = Services.GetRequiredService<IAuthService>();

            // 检查是否已登录
            if (authService.IsAuthenticated)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = Services.GetRequiredService<MainWindowViewModel>()
                };
            }
            else
            {
                desktop.MainWindow = new LoginWindow
                {
                    DataContext = Services.GetRequiredService<LoginViewModel>()
                };
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // 构建配置
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // 从配置文件读取 API 地址
        var apiBaseUrl = configuration["ApiSettings:BaseUrl"];

        // HTTP Client配置
        services.AddHttpClient("AgvApi", client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Services
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IAgvApiService, AgvApiService>();
        services.AddSingleton<INavigationService, NavigationService>();

        // ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<AgvListViewModel>();
        services.AddTransient<TaskMonitorViewModel>();
    }
}

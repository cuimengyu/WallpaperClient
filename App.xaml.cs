using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using WallpaperClient.Services;
using WallpaperClient.ViewModels;
using WallpaperClient.Views;

namespace WallpaperClient;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// 服务提供者
    /// </summary>
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    /// <summary>
    /// 服务集合
    /// </summary>
    private readonly IServiceCollection _services = new ServiceCollection();

    /// <summary>
    /// 应用程序启动
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 配置日志
        ConfigureLogging();

        // 配置依赖注入
        ConfigureServices();

        // 创建主窗口
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();

        Log.Information("应用程序启动");
    }

    /// <summary>
    /// 应用程序退出
    /// </summary>
    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("应用程序退出");
        Log.CloseAndFlush();
        base.OnExit(e);
    }

    /// <summary>
    /// 配置日志
    /// </summary>
    private void ConfigureLogging()
    {
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WallpaperClient",
            "Logs",
            "log-.txt"
        );

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
            .CreateLogger();

        Log.Information("日志系统初始化完成");
    }

    /// <summary>
    /// 配置依赖注入服务
    /// </summary>
    private void ConfigureServices()
    {
        // 注册服务
        _services.AddSingleton<IWallhavenService, WallhavenService>();
        _services.AddSingleton<IDownloadService, DownloadService>();
        _services.AddSingleton<IDatabaseService, DatabaseService>();
        _services.AddSingleton<IWallpaperService, WallpaperService>();

        // 注册视图模型
        _services.AddSingleton<MainViewModel>();

        // 注册窗口和视图
        _services.AddSingleton<MainWindow>();

        // 构建服务提供者
        ServiceProvider = _services.BuildServiceProvider();

        Log.Information("依赖注入配置完成");
    }
}

using DormitoryManagement.Data;
using DormitoryManagement.Services;
using DormitoryManagement.ViewModels;
using DormitoryManagement.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Windows;

namespace DormitoryManagement;
public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; }
    private ILogger<App> _logger;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        ServiceProvider = serviceCollection.BuildServiceProvider();
        _logger = ServiceProvider.GetRequiredService<ILogger<App>>();

        try
        {
            // Применение миграций
            using var scope = ServiceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();

            // Запуск главного окна
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Application startup failed");
            MessageBox.Show("Произошла критическая ошибка при запуске приложения. Подробности в логах.",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Логирование
        services.AddLogging(configure =>
        {
            //configure.AddConsole();
           // configure.AddDebug();
        });

        // База данных
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql("Host=localhost;Database=dormitory_db;Username=postgres;Password=123");
           // options.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()));
        });

        // Регистрация сервисов
        services.AddScoped<IDormitoryService, DormitoryService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IStudentService, StudentService>();

        // ViewModels
        services.AddTransient<MainViewModel>();

        // Окна
        services.AddTransient<MainWindow>();
    }
}
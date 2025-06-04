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
            using var scope = ServiceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Простая проверка подключения
            if (await db.Database.CanConnectAsync())
            {
                await db.Database.MigrateAsync(); // Применяем миграции
                MessageBox.Show("Подключение к БД успешно!");
            }
            else
            {
                MessageBox.Show("Не удалось подключиться к БД");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка подключения: {ex.Message}");
            Shutdown();
            return;
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
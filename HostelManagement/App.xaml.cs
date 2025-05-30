using DormitoryManagement.Data;
using DormitoryManagement.Services;
using DormitoryManagement.ViewModels;
using DormitoryManagement.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace DormitoryManagement;
public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
        
        // Применяем миграции
        using var scope = ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>();
        
        services.AddSingleton<IDormitoryService, DormitoryService>();
        services.AddSingleton<IRoomService, RoomService>();
        services.AddSingleton<IStudentService, StudentService>();
        
        services.AddSingleton<MainViewModel>();
        services.AddTransient<MainWindow>();
    }
}
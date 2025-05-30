using DormitoryManagement.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace DormitoryManagement.Views;
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = App.ServiceProvider.GetRequiredService<MainViewModel>();
    }
}
using DormitoryManagement;
using HostelManagement.Services;
using HostelManagement.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace HostelManagement.Views
{
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<AuthViewModel>();

            PasswordBox.PasswordChanged += (sender, e) =>
            {
                if (DataContext is AuthViewModel vm)
                {
                    vm.Password = PasswordBox.Password;
                }
            };

        }
    }
}

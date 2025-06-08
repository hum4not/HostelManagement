using DormitoryManagement;
using HostelManagement.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
namespace HostelManagement.Views
{
    public partial class AddStudentDialog : Window
    {
        public AddStudentDialog()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<AddStudentDialogViewModel>();
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}

using DormitoryManagement;
using HostelManagement.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace DormitoryManagement.Views;
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

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(FullNameTextBox.Text) ||
           string.IsNullOrWhiteSpace(GroupTextBox.Text))
        {
            MessageBox.Show("Заполните все поля!");
            return;
        }

        DialogResult = true;
        Close();
    }
}

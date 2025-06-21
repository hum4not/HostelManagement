using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DormitoryManagement.Data;
using DormitoryManagement.Services;
using DormitoryManagement.Views;
using System.Windows;

namespace HostelManagement.ViewModels
{
    public partial class AddStudentDialogViewModel : ObservableObject
    {

        [ObservableProperty]
        private string _fullName = string.Empty;

        [ObservableProperty]
        private string _groupName = string.Empty;

        [ObservableProperty]
        private int _course = 1;

        [RelayCommand]
        private void Add()
        {

            if (Application.Current.Windows.OfType<AddStudentDialog>().FirstOrDefault() is AddStudentDialog dialog)
            {
                dialog.DialogResult = true;
            }
            
        }
    }

}

using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

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
    }
}

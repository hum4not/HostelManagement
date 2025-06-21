using DormitoryManagement.Views;
using HostelManagement.Models;
using HostelManagement.Services;
using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

public class AuthViewModel : INotifyPropertyChanged
{
    private readonly IUserRepository _userRepository;

    private string _username;
    private string _password;
    private string _role = "read";
    private BitmapImage _avatar;
    private bool _isRegisterMode;
    private string _statusMessage;

    public ICommand LoginCommand { get; }
    public ICommand RegisterCommand { get; }
    public ICommand SwitchModeCommand { get; }
    public ICommand UploadImageCommand { get; }

    public AuthViewModel(IUserRepository userRepository)
    {
        _userRepository = userRepository;

        // Кнопки всегда активны
        LoginCommand = new RelayCommand(async _ => await Login());
        RegisterCommand = new RelayCommand(async _ => await Register());
        SwitchModeCommand = new RelayCommand(_ => IsRegisterMode = !IsRegisterMode);
        UploadImageCommand = new RelayCommand(_ => UploadImage());
    }

    public string Username
    {
        get => _username;
        set { _username = value; OnPropertyChanged(); }
    }

    public string Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(); }
    }

    public string Role
    {
        get => _role;
        set { _role = value; OnPropertyChanged(); }
    }

    public BitmapImage Avatar
    {
        get => _avatar;
        set { _avatar = value; OnPropertyChanged(); }
    }

    public bool IsRegisterMode
    {
        get => _isRegisterMode;
        set { _isRegisterMode = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsLoginMode)); }
    }

    public bool IsLoginMode => !IsRegisterMode;

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    private async Task Login()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            StatusMessage = "Введите логин и пароль";
            MessageBox.Show("Ошибка: Логин и пароль обязательны", "Ошибка");
            return;
        }

        var success = await _userRepository.Authenticate(Username, Password);
        if (success)
        {
            StatusMessage = "Успешный вход!";
            
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
        else
        {
            StatusMessage = "Неверное имя пользователя или пароль";
            MessageBox.Show("Ошибка авторизации", "Ошибка");
        }
    }

    private async Task Register()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(Role))
        {
            StatusMessage = "Заполните все поля";
            MessageBox.Show("Ошибка: Все поля обязательны", "Ошибка");
            return;
        }

        if (await _userRepository.UsernameExists(Username))
        {
            StatusMessage = "Имя пользователя уже занято";
            MessageBox.Show("Ошибка: Пользователь уже существует", "Ошибка");
            return;
        }

        byte[] pictureBytes = null;
        if (Avatar != null)
        {
            using var memoryStream = new MemoryStream();
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(Avatar));
            encoder.Save(memoryStream);
            pictureBytes = memoryStream.ToArray();
        }

        var user = new User
        {
            Username = Username,
            Password = Password,
            Role = Role,
            Picture = pictureBytes
        };

        var success = await _userRepository.Register(user);
        if (success)
        {
            StatusMessage = "Регистрация успешна!";
            MessageBox.Show("Регистрация успешна!", "Успех");
            IsRegisterMode = false;
        }
        else
        {
            StatusMessage = "Ошибка при регистрации";
            MessageBox.Show("Ошибка при регистрации", "Ошибка");
        }
    }

    private void UploadImage()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*",
            Title = "Выберите аватарку"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(openFileDialog.FileName);
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                Avatar = image;
                MessageBox.Show("Аватар успешно загружен", "Успех");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки изображения: {ex.Message}";
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;

    public RelayCommand(Action<object> execute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    }

    public bool CanExecute(object parameter) => true;

    public void Execute(object parameter) => _execute(parameter);

    public event EventHandler CanExecuteChanged
    {
        add { }
        remove { }
    }
}
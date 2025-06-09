using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DormitoryManagement.Data;
using DormitoryManagement.Models;
using DormitoryManagement.Services;
using DormitoryManagement.Views;
using HostelManagement.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;


namespace DormitoryManagement.ViewModels;
public partial class MainViewModel : ObservableObject
{
    private readonly IDormitoryService _dormitoryService;
    private readonly IRoomService _roomService;
    private readonly IStudentService _studentService;
    private readonly ILogger<MainViewModel> _logger;

    [ObservableProperty] private bool _isMenuExpanded = true;
    [ObservableProperty] private ObservableCollection<Dormitory> _dormitories = new();
    [ObservableProperty] private ObservableCollection<Room> _rooms = new();
    [ObservableProperty] private ObservableCollection<Student> _students = new();
    [ObservableProperty] private Dormitory? _selectedDormitory;
    [ObservableProperty] private Room? _selectedRoom;
    [ObservableProperty] private Student? _selectedStudent;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private bool _isEditMode;
    [ObservableProperty] private string _statusMessage = string.Empty;

    [ObservableProperty] private int _totalRooms;
    [ObservableProperty] private int _totalStudents;
    [ObservableProperty] private int _availableSpaces;

    [ObservableProperty] private string _editDormitoryName;
    [ObservableProperty] private string _editDormitoryAddress;
    [ObservableProperty] private string _editRoomNumber;

    [ObservableProperty] private string _studentSearchText = "";
    [ObservableProperty] private ObservableCollection<Student> _searchStudentsResults = new();

    [ObservableProperty] private ObservableCollection<Student> _currentRoomStudents = new();

    public MainViewModel(
        IDormitoryService dormitoryService,
        IRoomService roomService,
        IStudentService studentService,
        ILogger<MainViewModel> logger)
    {
        _dormitoryService = dormitoryService;
        _roomService = roomService;
        _studentService = studentService;
        _logger = logger;

        LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
        AddDormitoryCommand = new AsyncRelayCommand(AddDormitoryAsync);
        EditDormitoryCommand = new AsyncRelayCommand(EditDormitoryAsync);
        DeleteDormitoryCommand = new AsyncRelayCommand(DeleteDormitoryAsync);
        AddRoomCommand = new AsyncRelayCommand(AddRoomAsync);
        EditRoomCommand = new AsyncRelayCommand(EditRoomAsync);
        DeleteRoomCommand = new AsyncRelayCommand(DeleteRoomAsync);
        EditStudentCommand = new AsyncRelayCommand(EditStudentAsync);
        DeleteStudentCommand = new AsyncRelayCommand(DeleteStudentAsync);
        AccommodateStudentCommand = new AsyncRelayCommand(AccommodateStudent);
        EvictStudentCommand = new AsyncRelayCommand(EvictStudentAsync);
        TransferStudentCommand = new AsyncRelayCommand(TransferStudentAsync);
        MoveAllToNextCourseCommand = new AsyncRelayCommand(MoveAllToNextCourseAsync);
        SearchCommand = new AsyncRelayCommand(SearchAsync);

        LoadDataAsync().ConfigureAwait(false);
    }


    public ICommand LoadDataCommand { get; }
    public ICommand AddDormitoryCommand { get; }
    public ICommand EditDormitoryCommand { get; }
    public ICommand DeleteDormitoryCommand { get; }
    public ICommand AddRoomCommand { get; }
    public ICommand EditRoomCommand { get; }
    public ICommand DeleteRoomCommand { get; }
    public ICommand AddStudentCommand { get; }
    public ICommand EditStudentCommand { get; }
    public ICommand DeleteStudentCommand { get; }
    public ICommand AccommodateStudentCommand { get; }
    public ICommand EvictStudentCommand { get; }
    public ICommand TransferStudentCommand { get; }
    public ICommand MoveAllToNextCourseCommand { get; }
    public ICommand SearchCommand { get; }

    [RelayCommand]
    private void PromoteAllStudents()
    {
        _studentService.PromoteAllStudents();
        LoadCurrentRoomStudents();
        MessageBox.Show("Все студенты переведены на следующий курс!");
    }


    private async Task LoadCurrentRoomStudents()
    {
        if (SelectedRoom != null)
        {
            CurrentRoomStudents = new ObservableCollection<Student>(
                _studentService.GetStudentsByRoom(SelectedRoom.Id));
        }
    }

    private async void LoadDormitories()
    {
        Dormitories = new ObservableCollection<Dormitory>(
            await _dormitoryService.GetDormitoriesAsync());
    }

    partial void OnSelectedDormitoryChanged(Dormitory? value)
    {
        LoadRoomsForSelectedDormitory();

    }

    partial void OnSelectedRoomChanged(Room? value)
    {
        LoadCurrentRoomStudents();
    }

    private async void LoadRoomsForSelectedDormitory()
    {
        if (SelectedDormitory != null)
        {
            Rooms = new ObservableCollection<Room>(
                await _roomService.GetRoomsAsync(SelectedDormitory.Id));
        }
        else
        {
            Rooms.Clear();
        }
    }

    [RelayCommand]
    private async Task SaveChangesAsync()
    {
        try
        {
            await SaveChangesAsync();
            StatusMessage = "Изменения сохранены";

            if (SelectedDormitory != null)
                LoadRoomsForSelectedDormitory();

            if (SelectedRoom != null)
                LoadCurrentRoomStudents();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveDormitoryChanges()
    {
        if (SelectedDormitory == null) return;

        try
        {
            SelectedDormitory.Name = EditDormitoryName;
            SelectedDormitory.Address = EditDormitoryAddress;

            await _dormitoryService.UpdateDormitoryAsync(SelectedDormitory);
            StatusMessage = "Общежитие успешно обновлено";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveRoomChanges()
    {
        if (SelectedRoom == null) return;

        try
        {
            SelectedRoom.Number = EditRoomNumber;
            await _roomService.UpdateRoomAsync(SelectedRoom);
            StatusMessage = "Комната успешно обновлена";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ToggleMenu() => IsMenuExpanded = !IsMenuExpanded;

    [RelayCommand]
    private async Task ShowAddStudentDialog()
    {
        var dialog = App.ServiceProvider.GetRequiredService<AddStudentDialog>();
        dialog.Owner = Application.Current.MainWindow;

        if (dialog.ShowDialog() == true)
        {
            var vm = (AddStudentDialogViewModel)dialog.DataContext;
            try
            {
                await _studentService.AddStudentAsync(
                    vm.FullName,
                    vm.GroupName,
                    vm.Course);

                StatusMessage = $"Студент {vm.FullName} добавлен!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
            }
        }
    }



    [RelayCommand]
    private async Task TestDbConnection()
    {

        //var owner = this.Owner;
        //var dialogResult = this.DialogResult;
        //var dataContext = this.DataContext;

        //// Закрываем текущее окно
        //this.Close();

        //// Создаем новое окно
        //var newWindow = new AddStudentDialog()
        //{
        //    Owner = owner,
        //    DataContext = dataContext
        //};

        //// Показываем новое окно
        //newWindow.ShowDialog();
    
    }



    [RelayCommand]
    private async Task ShowStudentHistoryAsync()
    {
        if (SelectedStudent == null) return;

        try
        {
            var history = await _studentService.GetStudentHistoryAsync(SelectedStudent.Id);
            StatusMessage = $"Показана история для {SelectedStudent.FullName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке истории");
            StatusMessage = "Ошибка при загрузке истории";
        }
    }

    [RelayCommand]
    private async Task SearchStudents()
    {
        if (string.IsNullOrWhiteSpace(StudentSearchText))
        {
            SearchStudentsResults.Clear();
            return;
        }

        var results = await _studentService.SearchStudentsAsync(StudentSearchText);
        SearchStudentsResults = new ObservableCollection<Student>(results);
    }



    [RelayCommand]
    private async Task AddStudentToRoom()
    {
        if (SelectedStudent == null || SelectedRoom == null) return;

        if (SelectedRoom.Students.Count >= SelectedRoom.Capacity)
        {
            StatusMessage = "Комната заполнена!";
            return;
        }

        await _studentService.AccommodateStudent(
            SelectedStudent.Id,
            SelectedRoom.Id);

        StatusMessage = $"Студент {SelectedStudent.FullName} заселен в комнату {SelectedRoom.Number}";
    }



    private async Task LoadDataAsync()
    {
        try
        {
            var dormitories = await _dormitoryService.GetDormitoriesAsync();
            Dormitories = new ObservableCollection<Dormitory>(dormitories);

            if (SelectedDormitory != null)
            {
                await LoadRoomsAsync(SelectedDormitory.Id);
            }

            UpdateStatistics();
            StatusMessage = "Данные успешно загружены";
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка при загрузке данных: {ex}");
            StatusMessage = "Ошибка при загрузке данных";
        }
    }


    private async Task LoadRoomsAsync(int dormitoryId)
    {
        var rooms = await _roomService.GetRoomsAsync(dormitoryId);
        Rooms = new ObservableCollection<Room>(rooms);

        if (SelectedRoom != null)
        {
            await LoadStudentsAsync(SelectedRoom.Id);
        }
    }

    private async void LoadStudentsForRoom()
    {
        if (SelectedRoom != null)
        {
            var students = await _roomService.GetStudentsInRoomAsync(SelectedRoom.Id);
            CurrentRoomStudents = new ObservableCollection<Student>(students);
        }
        else
        {
            CurrentRoomStudents.Clear();
        }
    }

    private async Task LoadStudentsAsync(int roomId)
    {
        var students = await _studentService.GetStudentsAsync(roomId);
        Students = new ObservableCollection<Student>(students);
    }

    private void UpdateStatistics()
    {
        TotalRooms = Dormitories.Sum(d => d.Rooms.Count);
        TotalStudents = Dormitories.Sum(d => d.Rooms.Sum(r => r.Students.Count));
        AvailableSpaces = Dormitories.Sum(d => d.Rooms.Sum(r => r.Capacity - r.Students.Count));
    }

    private async Task AddDormitoryAsync()
    {
        try
        {
            var dormitory = new Dormitory
            {
                Name = "Новое общежитие",
                Address = "Адрес"
            };

            await _dormitoryService.AddDormitoryAsync(dormitory);
            Dormitories.Add(dormitory);
            SelectedDormitory = dormitory;
            StatusMessage = "Общежитие успешно добавлено";
            UpdateStatistics();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении общежития");
            StatusMessage = "Ошибка при добавлении общежития";
        }
    }

    private async Task EditDormitoryAsync()
    {
        if (SelectedDormitory == null) return;

        try
        {
            await _dormitoryService.UpdateDormitoryAsync(SelectedDormitory);
            StatusMessage = "Общежитие успешно обновлено";
            UpdateStatistics();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении общежития");
            StatusMessage = "Ошибка при обновлении общежития";
        }
    }

    private async Task DeleteDormitoryAsync()
    {
        if (SelectedDormitory == null) return;

        try
        {
            if (!await _dormitoryService.CanDeleteDormitoryAsync(SelectedDormitory.Id))
            {
                StatusMessage = "Нельзя удалить общежитие с заселенными студентами";
                return;
            }

            await _dormitoryService.DeleteDormitoryAsync(SelectedDormitory.Id);
            Dormitories.Remove(SelectedDormitory);
            SelectedDormitory = null;
            Rooms.Clear();
            Students.Clear();
            StatusMessage = "Общежитие успешно удалено";
            UpdateStatistics();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении общежития");
            StatusMessage = "Ошибка при удалении общежития";
        }
    }

    private async Task AddRoomAsync()
    {
        if (SelectedDormitory == null) return;

        try
        {
            var room = new Room
            {
                DormitoryId = SelectedDormitory.Id,
                Number = $"Номер {Rooms.Count + 1}",
                Capacity = 2
            };

            await _roomService.AddRoomAsync(room);
            Rooms.Add(room);
            SelectedRoom = room;
            StatusMessage = "Комната успешно добавлена";
            UpdateStatistics();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении комнаты");
            StatusMessage = "Ошибка при добавлении комнаты";
        }
    }

    private async Task EditRoomAsync()
    {
        if (SelectedRoom == null) return;

        try
        {
            await _roomService.UpdateRoomAsync(SelectedRoom);
            StatusMessage = "Комната успешно обновлена";
            UpdateStatistics();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении комнаты");
            StatusMessage = "Ошибка при обновлении комнаты";
        }
    }

    private async Task DeleteRoomAsync()
    {
        if (SelectedRoom == null) return;

        try
        {
            if (!await _roomService.CanDeleteRoomAsync(SelectedRoom.Id))
            {
                StatusMessage = "Нельзя удалить комнату с заселенными студентами";
                return;
            }

            await _roomService.DeleteRoomAsync(SelectedRoom.Id);
            Rooms.Remove(SelectedRoom);
            SelectedRoom = null;
            Students.Clear();
            StatusMessage = "Комната успешно удалена";
            UpdateStatistics();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении комнаты");
            StatusMessage = "Ошибка при удалении комнаты";
        }
    }

   

    private async Task EditStudentAsync()
    {
        if (SelectedStudent == null) return;

        try
        {
            await _studentService.UpdateStudentAsync(SelectedStudent);
            StatusMessage = "Студент успешно обновлен";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении студента");
            StatusMessage = "Ошибка при обновлении студента";
        }
    }

    private async Task DeleteStudentAsync()
    {
        if (SelectedStudent == null) return;

        try
        {
            await _studentService.DeleteStudentAsync(SelectedStudent.Id);
            Students.Remove(SelectedStudent);
            SelectedStudent = null;
            StatusMessage = "Студент успешно удален";
            UpdateStatistics();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении студента");
            StatusMessage = "Ошибка при удалении студента";
        }
    }

    private async Task AccommodateStudent()
    {
        if (SelectedStudent == null || SelectedRoom == null) return;

        await _studentService.AccommodateStudent(SelectedStudent.Id, SelectedRoom.Id);
        await LoadCurrentRoomStudents();
        MessageBox.Show("Студент заселен!");
    }

    private async Task EvictStudentAsync()
    {
        if (SelectedStudent == null) return;

        try
        {
            await _studentService.EvictStudentAsync(SelectedStudent.Id);
            await LoadStudentsAsync(SelectedRoom?.Id ?? 0);
            StatusMessage = "Студент успешно выселен";
            UpdateStatistics();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при выселении студента");
            StatusMessage = "Ошибка при выселении студента";
        }
    }

    private async Task TransferStudentAsync()
    {
        if (SelectedStudent == null || SelectedRoom == null) return;

        try
        {
            await _roomService.TransferStudentAsync(SelectedStudent.Id, SelectedRoom.Id);
            await LoadStudentsAsync(SelectedRoom.Id);
            StatusMessage = "Студент успешно переведен";
            UpdateStatistics();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при переводе студента");
            StatusMessage = "Ошибка при переводе студента";
        }
    }

    private async Task MoveAllToNextCourseAsync()
    {
        try
        {
            await _studentService.MoveStudentsToNextCourseAsync();
            await LoadDataAsync();
            StatusMessage = "Все студенты переведены на следующий курс";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при переводе студентов");
            StatusMessage = "Ошибка при переводе студентов";
        }
    }

    private async Task SearchAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadDataAsync();
                return;
            }

            var filteredStudents = await _studentService.SearchStudentsAsync(SearchText);
            Students = new ObservableCollection<Student>(filteredStudents);
            StatusMessage = $"Найдено {Students.Count} студентов";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при поиске");
            StatusMessage = "Ошибка при поиске";
        }
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DormitoryManagement.Data;
using DormitoryManagement.Models;
using DormitoryManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        AddStudentCommand = new AsyncRelayCommand(AddStudentAsync);
        EditStudentCommand = new AsyncRelayCommand(EditStudentAsync);
        DeleteStudentCommand = new AsyncRelayCommand(DeleteStudentAsync);
        AccommodateStudentCommand = new AsyncRelayCommand(AccommodateStudentAsync);
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
    private void ToggleMenu() => IsMenuExpanded = !IsMenuExpanded;

    [RelayCommand]
    private async Task TestDbConnection()
    {
        try
        {
            using var scope = App.ServiceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Проверка подключения
            bool canConnect = await db.Database.CanConnectAsync();
            Debug.WriteLine($"Can connect: {canConnect}");

            // 2. Прямое добавление тестовых данных
            var testDorm = new Dormitory
            {
                Name = "Тест " + DateTime.Now.ToString("HH:mm:ss"),
                Address = "Тестовый адрес"
            };

            db.Dormitories.Add(testDorm);
            int affected = await db.SaveChangesAsync();
            Debug.WriteLine($"Affected rows: {affected}");

            // 3. Проверка данных
            var count = await db.Dormitories.CountAsync();
            Debug.WriteLine($"Total dorms: {count}");

            StatusMessage = $"Тест выполнен. Добавлено: {affected} записей";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ОШИБКА: {ex.ToString()}");
            StatusMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task ShowStudentHistoryAsync()
    {
        if (SelectedStudent == null) return;

        try
        {
            var history = await _studentService.GetStudentHistoryAsync(SelectedStudent.Id);
            // Реализация отображения истории
            StatusMessage = $"Показана история для {SelectedStudent.FullName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке истории");
            StatusMessage = "Ошибка при загрузке истории";
        }
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
            //await _roomService.UpdateRoomAsync(SelectedRoom);
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

    private async Task AddStudentAsync()
    {
        try
        {
            var student = new Student
            {
                FullName = "Новый студент",
                GroupName = "Группа",
                Course = 1
            };

            await _studentService.AddStudentAsync(student);
            Students.Add(student);
            SelectedStudent = student;
            StatusMessage = "Студент успешно добавлен";
            UpdateStatistics();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении студента");
            StatusMessage = "Ошибка при добавлении студента";
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

    private async Task AccommodateStudentAsync()
    {
        if (SelectedStudent == null || SelectedRoom == null) return;

        try
        {
            await _studentService.AccommodateStudentAsync(SelectedStudent.Id, SelectedRoom.Id);
            await LoadStudentsAsync(SelectedRoom.Id);
            StatusMessage = "Студент успешно заселен";
            UpdateStatistics();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при заселении студента");
            StatusMessage = "Ошибка при заселении студента";
        }
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


    partial void OnSelectedDormitoryChanged(Dormitory? value)
    {
        if (value != null)
        {
            LoadRoomsAsync(value.Id).ConfigureAwait(false);
        }
        else
        {
            Rooms.Clear();
            Students.Clear();
        }
    }

    partial void OnSelectedRoomChanged(Room? value)
    {
        if (value != null)
        {
            LoadStudentsAsync(value.Id).ConfigureAwait(false);
        }
        else
        {
            Students.Clear();
        }
    }
}
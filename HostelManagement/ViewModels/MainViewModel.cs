using DormitoryManagement.Services;
using DormitoryManagement.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DormitoryManagement.ViewModels;
public partial class MainViewModel : ObservableObject
{
    private readonly IDormitoryService _dormitoryService;
    private readonly IRoomService _roomService;
    private readonly IStudentService _studentService;
    
    [ObservableProperty] private bool _isMenuExpanded = true;
    [ObservableProperty] private ObservableCollection<Dormitory> _dormitories;
    [ObservableProperty] private ObservableCollection<Room> _rooms;
    [ObservableProperty] private ObservableCollection<Student> _students;
    [ObservableProperty] private Dormitory _selectedDormitory;
    [ObservableProperty] private Room _selectedRoom;
    
    public MainViewModel(
        IDormitoryService dormitoryService,
        IRoomService roomService,
        IStudentService studentService)
    {
        _dormitoryService = dormitoryService;
        _roomService = roomService;
        _studentService = studentService;
        
        LoadDataAsync().ConfigureAwait(false);
    }
    
    private async Task LoadDataAsync()
    {
        Dormitories = new ObservableCollection<Dormitory>(await _dormitoryService.GetDormitoriesAsync());
    }
    
    [RelayCommand]
    private void ToggleMenu() => IsMenuExpanded = !IsMenuExpanded;
    
    [RelayCommand]
    private async Task SelectDormitoryAsync(Dormitory dormitory)
    {
        SelectedDormitory = dormitory;
        if (dormitory != null)
        {
            Rooms = new ObservableCollection<Room>(await _roomService.GetRoomsAsync(dormitory.Id));
        }
    }
    
    [RelayCommand]
    private async Task SelectRoomAsync(Room room)
    {
        SelectedRoom = room;
        if (room != null)
        {
            Students = new ObservableCollection<Student>(await _studentService.GetStudentsAsync(room.Id));
        }
    }
    
    [RelayCommand]
    private async Task AddDormitoryAsync()
    {
        var dormitory = new Dormitory { Name = "Новое общежитие", Address = "Адрес" };
        await _dormitoryService.AddDormitoryAsync(dormitory);
        Dormitories.Add(dormitory);
    }
    
    [RelayCommand]
    private async Task DeleteDormitoryAsync()
    {
        if (SelectedDormitory == null) return;
        
        if (await _dormitoryService.CanDeleteDormitoryAsync(SelectedDormitory.Id))
        {
            await _dormitoryService.DeleteDormitoryAsync(SelectedDormitory.Id);
            Dormitories.Remove(SelectedDormitory);
            SelectedDormitory = null;
            Rooms?.Clear();
            Students?.Clear();
        }
    }
    
    [RelayCommand]
    private async Task AddRoomAsync()
    {
        if (SelectedDormitory == null) return;
        
        var room = new Room { 
            DormitoryId = SelectedDormitory.Id,
            Number = "Номер",
            Capacity = 2 
        };
        
        await _roomService.AddRoomAsync(room);
        Rooms.Add(room);
    }
    
    [RelayCommand]
    private async Task DeleteRoomAsync()
    {
        if (SelectedRoom == null) return;
        
        if (await _roomService.CanDeleteRoomAsync(SelectedRoom.Id))
        {
            await _roomService.DeleteRoomAsync(SelectedRoom.Id);
            Rooms.Remove(SelectedRoom);
            SelectedRoom = null;
            Students?.Clear();
        }
    }
    
    [RelayCommand]
    private async Task AccommodateStudentAsync(Student student)
    {
        if (SelectedRoom == null || student == null) return;
        
        await _studentService.AccommodateStudentAsync(student.Id, SelectedRoom.Id);
        await SelectRoomAsync(SelectedRoom); // Обновляем список
    }
    
    [RelayCommand]
    private async Task EvictStudentAsync(Student student)
    {
        if (student == null) return;
        
        await _studentService.EvictStudentAsync(student.Id);
        await SelectRoomAsync(SelectedRoom); // Обновляем список
    }
    
    [RelayCommand]
    private async Task TransferStudentAsync(Student student)
    {
        if (student == null || SelectedRoom == null) return;
        
        await _roomService.TransferStudentAsync(student.Id, SelectedRoom.Id);
        await SelectRoomAsync(SelectedRoom); // Обновляем список
    }
    
    [RelayCommand]
    private async Task MoveAllToNextCourseAsync()
    {
        await _studentService.MoveStudentsToNextCourseAsync();
        if (SelectedRoom != null)
        {
            await SelectRoomAsync(SelectedRoom);
        }
    }
}
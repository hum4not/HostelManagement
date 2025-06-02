using DormitoryManagement.Models;

namespace DormitoryManagement.Services;
public interface IRoomService
{
    Task<List<Room>> GetRoomsAsync(int dormitoryId);
    //Task<Room?> GetRoomByIdAsync(int id);
    Task AddRoomAsync(Room room);
    //Task UpdateRoomAsync(Room room);
    Task DeleteRoomAsync(int id);
    Task<bool> CanDeleteRoomAsync(int id);
    Task<bool> HasFreeSpaceAsync(int roomId);
    Task<int> GetFreeSpacesCountAsync(int roomId);
    Task TransferStudentAsync(int studentId, int newRoomId);
    //Task<List<Room>> GetAvailableRoomsAsync(int dormitoryId);
}
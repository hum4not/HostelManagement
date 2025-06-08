using DormitoryManagement.Models;

namespace DormitoryManagement.Services;
public interface IRoomService
{
    Task<List<Room>> GetRoomsAsync(int dormitoryId);
    Task AddRoomAsync(Room room);
    Task DeleteRoomAsync(int id);
    Task<bool> CanDeleteRoomAsync(int id);
    Task<bool> HasFreeSpaceAsync(int roomId);
    Task<int> GetFreeSpacesCountAsync(int roomId);
    Task TransferStudentAsync(int studentId, int newRoomId);
    Task UpdateRoomAsync(Room room);
}
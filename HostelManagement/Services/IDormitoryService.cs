using DormitoryManagement.Models;

namespace DormitoryManagement.Services;
public interface IDormitoryService
{
    Task<List<Dormitory>> GetDormitoriesAsync();
    Task<Dormitory?> GetDormitoryByIdAsync(int id);
    Task AddDormitoryAsync(Dormitory dormitory);
    Task UpdateDormitoryAsync(Dormitory dormitory);
    Task DeleteDormitoryAsync(int id);
    Task<bool> CanDeleteDormitoryAsync(int id);
    Task<int> GetTotalRoomsCountAsync(int dormitoryId);
    Task<int> GetTotalStudentsCountAsync(int dormitoryId);

}
using DormitoryManagement.Models;

namespace DormitoryManagement.Services;
public interface IDormitoryService
{
    Task<List<Dormitory>> GetDormitoriesAsync();
    Task AddDormitoryAsync(Dormitory dormitory);
    Task DeleteDormitoryAsync(int id);
    Task<bool> CanDeleteDormitoryAsync(int id);
}
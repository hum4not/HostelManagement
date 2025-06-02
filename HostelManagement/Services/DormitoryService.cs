using DormitoryManagement.Data;
using DormitoryManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DormitoryManagement.Services;
public class DormitoryService : IDormitoryService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DormitoryService> _logger;

    public DormitoryService(ApplicationDbContext context, ILogger<DormitoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Dormitory>> GetDormitoriesAsync()
    {
        try
        {
            return await _context.Dormitories
                .Include(d => d.Rooms)
                .ThenInclude(r => r.Students)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting dormitories");
            throw;
        }
    }

    public async Task<Dormitory?> GetDormitoryByIdAsync(int id) =>
        await _context.Dormitories.FindAsync(id);

    public async Task AddDormitoryAsync(Dormitory dormitory)
    {
        try
        {
            await _context.Dormitories.AddAsync(dormitory);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Added new dormitory: {DormitoryName}", dormitory.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while adding dormitory");
            throw;
        }
    }

    public async Task UpdateDormitoryAsync(Dormitory dormitory)
    {
        try
        {
            _context.Dormitories.Update(dormitory);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating dormitory");
            throw;
        }
    }

    public async Task DeleteDormitoryAsync(int id)
    {
        try
        {
            var dormitory = await _context.Dormitories.FindAsync(id);
            if (dormitory != null)
            {
                _context.Dormitories.Remove(dormitory);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted dormitory with ID: {DormitoryId}", id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting dormitory");
            throw;
        }
    }

    public async Task<bool> CanDeleteDormitoryAsync(int id) =>
        !await _context.Rooms.AnyAsync(r => r.DormitoryId == id && r.Students.Any());

    public async Task<int> GetTotalRoomsCountAsync(int dormitoryId) =>
        await _context.Rooms.CountAsync(r => r.DormitoryId == dormitoryId);

    public async Task<int> GetTotalStudentsCountAsync(int dormitoryId) =>
        await _context.Students.CountAsync(s => s.Room != null && s.Room.DormitoryId == dormitoryId);
}
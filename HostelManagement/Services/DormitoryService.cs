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
            _logger.LogError($"Error while getting dormitories {ex}");
            throw;
        }
    }

    public async Task<Dormitory?> GetDormitoryByIdAsync(int id) =>
        await _context.Dormitories.FindAsync(id);

    public async Task AddDormitoryAsync(Dormitory dormitory)
    {
        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

            await _context.Dormitories.AddAsync(dormitory);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            int affected = await _context.SaveChangesAsync();

            if (affected == 0)
                throw new Exception("Ни одна запись не была сохранена");

            _logger.LogInformation($"Добавлено общежитие ID: {dormitory.Id}");
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Ошибка сохранения в БД");
            throw new Exception("Ошибка при сохранении в базу данных", dbEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Неизвестная ошибка");
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
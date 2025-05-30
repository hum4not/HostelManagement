using DormitoryManagement.Data;
using DormitoryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Services;
public class DormitoryService : IDormitoryService
{
    private readonly ApplicationDbContext _context;
    
    public DormitoryService(ApplicationDbContext context) => _context = context;

    public async Task<List<Dormitory>> GetDormitoriesAsync() => 
        await _context.Dormitories.Include(d => d.Rooms).ToListAsync();

    public async Task AddDormitoryAsync(Dormitory dormitory)
    {
        _context.Dormitories.Add(dormitory);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteDormitoryAsync(int id)
    {
        var dormitory = await _context.Dormitories.FindAsync(id);
        if (dormitory != null)
        {
            _context.Dormitories.Remove(dormitory);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> CanDeleteDormitoryAsync(int id) => 
        !await _context.Rooms.AnyAsync(r => r.DormitoryId == id && r.Students.Any());
}
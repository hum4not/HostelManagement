using DormitoryManagement.Data;
using DormitoryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Services;
public class RoomService : IRoomService
{
    private readonly ApplicationDbContext _context;
    
    public RoomService(ApplicationDbContext context) => _context = context;

    public async Task<List<Room>> GetRoomsAsync(int dormitoryId) => 
        await _context.Rooms
            .Where(r => r.DormitoryId == dormitoryId)
            .Include(r => r.Students)
            .ToListAsync();

    public async Task AddRoomAsync(Room room)
    {
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteRoomAsync(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room != null)
        {
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
        }
    }
    public async Task UpdateRoomAsync(Room room)
    {
        _context.Rooms.Update(room);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> CanDeleteRoomAsync(int id) => 
        !await _context.Students.AnyAsync(s => s.RoomId == id);

    public async Task<bool> HasFreeSpaceAsync(int roomId)
    {
        var room = await _context.Rooms.FindAsync(roomId);
        return room != null && room.CurrentOccupancy < room.Capacity;
    }

    public async Task<int> GetFreeSpacesCountAsync(int roomId)
    {
        var room = await _context.Rooms.FindAsync(roomId);
        return room?.Capacity - room?.CurrentOccupancy ?? 0;
    }

    public async Task<List<Student>> GetStudentsInRoomAsync(int roomId)
    {
        return await _context.Students
            .Where(s => s.RoomId == roomId)
            .Include(s => s.Room)
            .ToListAsync();
    }

    public async Task TransferStudentAsync(int studentId, int newRoomId)
    {
        var student = await _context.Students.FindAsync(studentId);
        if (student == null) return;

        var oldRoomId = student.RoomId;
        var newRoom = await _context.Rooms.FindAsync(newRoomId);
        
        if (newRoom == null || newRoom.CurrentOccupancy >= newRoom.Capacity) return;

        var currentAccommodation = await _context.AccommodationHistory
            .FirstOrDefaultAsync(a => a.StudentId == studentId && a.CheckOutDate == null);
        
        if (currentAccommodation != null)
        {
            currentAccommodation.CheckOutDate = DateTime.Now;
        }

        _context.AccommodationHistory.Add(new AccommodationHistory
        {
            StudentId = studentId,
            RoomId = newRoomId,
            CheckInDate = DateTime.Now
        });

        if (oldRoomId.HasValue)
        {
            var oldRoom = await _context.Rooms.FindAsync(oldRoomId);
            if (oldRoom != null) oldRoom.CurrentOccupancy--;
        }

        newRoom.CurrentOccupancy++;
        student.RoomId = newRoomId;
        
        await _context.SaveChangesAsync();
    }
}
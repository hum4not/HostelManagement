using DormitoryManagement.Data;
using DormitoryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Services;
public class StudentService : IStudentService
{
    private readonly ApplicationDbContext _context;
    private readonly IRoomService _roomService;
    
    public StudentService(ApplicationDbContext context, IRoomService roomService)
    {
        _context = context;
        _roomService = roomService;
    }

    public async Task<List<Student>> GetStudentsAsync(int? roomId = null)
    {
        var query = _context.Students.Include(s => s.Room).AsQueryable();
        if (roomId.HasValue) query = query.Where(s => s.RoomId == roomId);
        return await query.ToListAsync();
    }

    public async Task AddStudentAsync(Student student)
    {
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteStudentAsync(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student != null)
        {
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AccommodateStudentAsync(int studentId, int roomId)
    {
        if (!await _roomService.HasFreeSpaceAsync(roomId)) return;
        
        var student = await _context.Students.FindAsync(studentId);
        if (student == null) return;

        student.RoomId = roomId;
        
        _context.AccommodationHistory.Add(new AccommodationHistory
        {
            StudentId = studentId,
            RoomId = roomId,
            CheckInDate = DateTime.Now
        });

        var room = await _context.Rooms.FindAsync(roomId);
        if (room != null) room.CurrentOccupancy++;
        
        await _context.SaveChangesAsync();
    }

    public async Task EvictStudentAsync(int studentId)
    {
        var student = await _context.Students.FindAsync(studentId);
        if (student == null || !student.RoomId.HasValue) return;

        var room = await _context.Rooms.FindAsync(student.RoomId);
        if (room != null) room.CurrentOccupancy--;

        var accommodation = await _context.AccommodationHistory
            .FirstOrDefaultAsync(a => a.StudentId == studentId && a.CheckOutDate == null);
        
        if (accommodation != null)
        {
            accommodation.CheckOutDate = DateTime.Now;
        }

        student.RoomId = null;
        await _context.SaveChangesAsync();
    }

    public async Task MoveStudentsToNextCourseAsync()
    {
        var students = await _context.Students.ToListAsync();
        foreach (var student in students)
        {
            student.Course++;
        }
        await _context.SaveChangesAsync();
    }

    public async Task ChangeStudentsGroupAsync(string oldGroup, string newGroup)
    {
        var students = await _context.Students
            .Where(s => s.GroupName == oldGroup)
            .ToListAsync();
        
        foreach (var student in students)
        {
            student.GroupName = newGroup;
        }
        
        await _context.SaveChangesAsync();
    }
}
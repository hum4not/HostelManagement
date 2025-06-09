using DormitoryManagement.Data;
using DormitoryManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DormitoryManagement.Services;
public class StudentService : IStudentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StudentService> _logger;
    private readonly IRoomService _roomService;

    public StudentService(
        ApplicationDbContext context,
        ILogger<StudentService> logger,
        IRoomService roomService)
    {
        _context = context;
        _logger = logger;
        _roomService = roomService;
    }

    public async Task<List<Student>> GetStudentsAsync(int? roomId = null)
    {
        try
        {
            var query = _context.Students
                .Include(s => s.Room)
                .AsQueryable();

            if (roomId.HasValue)
            {
                query = query.Where(s => s.RoomId == roomId);
            }

            return await query.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting students");
            throw;
        }
    }

    public async Task<IEnumerable<Student>> GetAllStudentsAsync()
    {
        return await _context.Students.ToListAsync();
    }

    public async Task<Student?> GetStudentByIdAsync(int id)
    {
        return await _context.Students
            .Include(s => s.Room)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

 

    public async Task UpdateStudentAsync(Student student)
    {
        try
        {
            _context.Students.Update(student);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated student with ID: {StudentId}", student.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating student");
            throw;
        }
    }

    public async Task DeleteStudentAsync(int id)
    {
        try
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted student with ID: {StudentId}", id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting student");
            throw;
        }
    }

    public async Task<bool> CanDeleteStudentAsync(int id)
    {
        return !await _context.AccommodationHistory.AnyAsync(a => a.StudentId == id);
    }

    public async Task EvictStudentAsync(int studentId)
    {
        try
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null || !student.RoomId.HasValue)
                return;

            var room = await _context.Rooms.FindAsync(student.RoomId);
            if (room != null)
            {
                room.CurrentOccupancy--;
            }

            var accommodation = await _context.AccommodationHistory
                .FirstOrDefaultAsync(a => a.StudentId == studentId && a.CheckOutDate == null);

            if (accommodation != null)
            {
                accommodation.CheckOutDate = DateTime.Now;
            }

            student.RoomId = null;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Student {StudentId} evicted", studentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while evicting student");
            throw;
        }
    }

    public async Task MoveStudentsToNextCourseAsync()
    {
        try
        {
            var students = await _context.Students.ToListAsync();
            foreach (var student in students)
            {
                student.Course++;
            }
            await _context.SaveChangesAsync();
            _logger.LogInformation("All students moved to next course");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while moving students to next course");
            throw;
        }
    }

    public async Task ChangeStudentsGroupAsync(string oldGroup, string newGroup)
    {
        try
        {
            var students = await _context.Students
                .Where(s => s.GroupName == oldGroup)
                .ToListAsync();

            foreach (var student in students)
            {
                student.GroupName = newGroup;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Students group changed from {OldGroup} to {NewGroup}", oldGroup, newGroup);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while changing students group");
            throw;
        }
    }

    public async Task<List<Student>> SearchStudentsAsync(string searchTerm)
    {
        return await _context.Students
            .Where(s => s.FullName.Contains(searchTerm) ||
                       s.Id.ToString() == searchTerm)
            .ToListAsync();
    }

    public async Task AddStudentAsync(string fullName, string groupName, int course)
    {
        var student = new Student
        {
            FullName = fullName,
            GroupName = groupName,
            Course = course
        };

        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();
    }

    public async Task PromoteAllStudents()
    {
        var students = _context.Students.ToList();

        foreach (var student in students)
        {
            student.Course++;

            if (student.Course > 5)
            {
                _context.Students.Remove(student);
            }
        }

        _context.SaveChanges();
    }

    public List<Student> GetStudentsByRoom(int roomId)
    {
        return _context.Students
            .Where(s => s.RoomId == roomId)
            .ToList();
    }

    public async Task AccommodateStudent(int studentId, int roomId)
    {
        var student = _context.Students.Find(studentId);
        var room = _context.Rooms.Find(roomId);
    
        if (student != null && room != null)
        {
            student.RoomId = roomId;
            _context.SaveChanges();
        }
    }

    public async Task<List<AccommodationHistory>> GetStudentHistoryAsync(int studentId)
    {
        return await _context.AccommodationHistory
            .Include(a => a.Room)
            .Where(a => a.StudentId == studentId)
            .OrderByDescending(a => a.CheckInDate)
            .ToListAsync();
    }
}
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

    public async Task<Student?> GetStudentByIdAsync(int id)
    {
        return await _context.Students
            .Include(s => s.Room)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task AddStudentAsync(Student student)
    {
        try
        {
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Added new student: {StudentName}", student.FullName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while adding student");
            throw;
        }
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

    public async Task AccommodateStudentAsync(int studentId, int roomId)
    {
        try
        {
            if (!await _roomService.HasFreeSpaceAsync(roomId))
                throw new InvalidOperationException("Room is full");

            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
                throw new KeyNotFoundException("Student not found");

            // Если студент уже заселен, сначала выселяем
            if (student.RoomId.HasValue)
            {
                await EvictStudentAsync(studentId);
            }

            student.RoomId = roomId;

            _context.AccommodationHistory.Add(new AccommodationHistory
            {
                StudentId = studentId,
                RoomId = roomId,
                CheckInDate = DateTime.Now
            });

            var room = await _context.Rooms.FindAsync(roomId);
            if (room != null)
            {
                room.CurrentOccupancy++;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Student {StudentId} accommodated in room {RoomId}", studentId, roomId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while accommodating student");
            throw;
        }
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

    public async Task<List<Student>> SearchStudentsAsync(string searchText)
    {
        try
        {
            var searchTextLower = searchText.ToLower();
            return await _context.Students
                .Include(s => s.Room)
                .Where(s =>
                    s.FullName.ToLower().Contains(searchTextLower) ||
                    s.GroupName.ToLower().Contains(searchTextLower) ||
                    (s.Room != null && s.Room.Number.ToLower().Contains(searchTextLower)))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while searching students");
            throw;
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
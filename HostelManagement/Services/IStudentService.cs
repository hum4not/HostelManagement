using DormitoryManagement.Models;

namespace DormitoryManagement.Services;
public interface IStudentService
{
    Task<List<Student>> GetStudentsAsync(int? roomId = null);
    Task AddStudentAsync(Student student);
    Task DeleteStudentAsync(int id);
    Task AccommodateStudentAsync(int studentId, int roomId);
    Task EvictStudentAsync(int studentId);
    Task MoveStudentsToNextCourseAsync();
    Task ChangeStudentsGroupAsync(string oldGroup, string newGroup);
}
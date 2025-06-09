using DormitoryManagement.Models;

namespace DormitoryManagement.Services;
public interface IStudentService
{
    Task AddStudentAsync(string fullName, string groupName, int course);
    Task<IEnumerable<Student>> GetAllStudentsAsync();
    Task PromoteAllStudents();
    List<Student> GetStudentsByRoom(int roomId);
    Task AccommodateStudent(int studentId, int roomId);

    Task<List<Student>> GetStudentsAsync(int? roomId = null);
    Task<Student?> GetStudentByIdAsync(int id);
    Task UpdateStudentAsync(Student student);
    Task DeleteStudentAsync(int id);
    Task EvictStudentAsync(int studentId);
    Task MoveStudentsToNextCourseAsync();
    Task ChangeStudentsGroupAsync(string oldGroup, string newGroup);
    Task<List<Student>> SearchStudentsAsync(string searchText);
    Task<List<AccommodationHistory>> GetStudentHistoryAsync(int studentId);
}
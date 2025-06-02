using DormitoryManagement.Models;

namespace DormitoryManagement.Services;
public interface IStudentService
{
    Task<List<Student>> GetStudentsAsync(int? roomId = null);
    Task<Student?> GetStudentByIdAsync(int id);
    Task AddStudentAsync(Student student);
    Task UpdateStudentAsync(Student student);
    Task DeleteStudentAsync(int id);
    Task AccommodateStudentAsync(int studentId, int roomId);
    Task EvictStudentAsync(int studentId);
    Task MoveStudentsToNextCourseAsync();
    Task ChangeStudentsGroupAsync(string oldGroup, string newGroup);
    Task<List<Student>> SearchStudentsAsync(string searchText);
    Task<List<AccommodationHistory>> GetStudentHistoryAsync(int studentId);
}
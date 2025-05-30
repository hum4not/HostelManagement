namespace DormitoryManagement.Models;
public class Student
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string GroupName { get; set; }
    public int Course { get; set; }
    
    public int? RoomId { get; set; }
    public Room Room { get; set; }
    public ICollection<AccommodationHistory> AccommodationHistory { get; set; } = new List<AccommodationHistory>();
}
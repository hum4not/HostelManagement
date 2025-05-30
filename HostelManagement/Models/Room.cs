namespace DormitoryManagement.Models;
public class Room
{
    public int Id { get; set; }
    public string Number { get; set; }
    public int Capacity { get; set; }
    public int CurrentOccupancy { get; set; }
    
    public int DormitoryId { get; set; }
    public Dormitory Dormitory { get; set; }
    public ICollection<Student> Students { get; set; } = new List<Student>();
}
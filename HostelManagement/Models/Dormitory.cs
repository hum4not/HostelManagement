namespace DormitoryManagement.Models;
public class Dormitory
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
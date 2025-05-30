namespace DormitoryManagement.Models;
public class AccommodationHistory
{
    public int Id { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public string Notes { get; set; }
    
    public int StudentId { get; set; }
    public Student Student { get; set; }
    
    public int? RoomId { get; set; }
    public Room Room { get; set; }
}
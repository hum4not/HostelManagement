using System.ComponentModel.DataAnnotations;

namespace DormitoryManagement.Models;
public class Student
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; }

    [Required]
    [StringLength(10)]
    public string GroupName { get; set; }

    public int? Course { get; set; }
    public int? RoomId { get; set; }
    public Room? Room { get; set; }
}
using DormitoryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Data;
public class ApplicationDbContext : DbContext
{
    public DbSet<Dormitory> Dormitories { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<AccommodationHistory> AccommodationHistory { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Database=dormitory_db;Username=postgres;Password=123");
    }
}
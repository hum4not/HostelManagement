using DormitoryManagement.Models;
using HostelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Data;
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
         : base(options)
    {
    }
    public DbSet<User> Users { get; set; }
    public DbSet<Dormitory> Dormitories { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<AccommodationHistory> AccommodationHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
           .HasIndex(u => u.Username)
           .IsUnique();

        modelBuilder.Entity<Room>()
            .HasOne(r => r.Dormitory)
            .WithMany(d => d.Rooms)
            .HasForeignKey(r => r.DormitoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Student>()
            .HasOne(s => s.Room)
            .WithMany(r => r.Students)
            .HasForeignKey(s => s.RoomId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
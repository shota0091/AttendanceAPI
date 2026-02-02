using Attendance.Models;
using Microsoft.EntityFrameworkCore;

namespace Attendance.Data
{
    public class AppDbContext :DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }

        public DbSet<User> Users { get; set; }  
    }
}

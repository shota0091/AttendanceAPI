using System.ComponentModel.DataAnnotations;

namespace Attendance.DTOs
{
    public class UserRequest
    {
        [Required]
        public String UserId { get; set; } = string.Empty;
    }

    public class AttendanceUpdateRequest
    {
        [Required]
        public DateTime ClockInTime { get; set; }

        [Required]
        public DateTime ClockOutTime { get; set; }

        [Required]
        public string Note { get; set; } = string.Empty;

    }
}

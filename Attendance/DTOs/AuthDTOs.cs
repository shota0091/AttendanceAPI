using System.ComponentModel.DataAnnotations;

namespace Attendance.DTOs
{
    public class UserAuthRequest
    {
        [Required]
        public String UserName { get; set; }
        [Required]
        public String Password {  get; set; }
    }
}

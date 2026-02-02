using System.ComponentModel.DataAnnotations;

namespace Attendance.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        // ログインID (User001 とか)
        [Required]
        public string Username { get; set; }

        // 暗号化されたパスワード
        // (絶対に生パスワードを入れない！)
        [Required]
        public string PasswordHash { get; set; }
    }
}
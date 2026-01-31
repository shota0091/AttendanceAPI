using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Attendance.Models
{
    [Table("Attendance")]
    public class AttendanceRecord
    {
        [Key]
        // ID
        public int Id {  get; set; }

        [Required]
        [MaxLength(100)]
        // ユーザーID Todo:ログイン機能実装時に外部キー参照を行う
        public string UserId { get; set; } = string.Empty;

        [Required]
        // 勤務日
        public DateTime WorkDate { get; set; }

        [Required]
        // 出勤時間
        public DateTime ClockInTime { get; set; }

        // 退勤時間
        public DateTime? ClockOutTime { get; set; }

        [Required]
        // 申請フラグ:0:通常, 1:申請中, 2:承認済, 9:却下
        public int ApprovalStatus { get; set; } = 0;

        [MaxLength(500)]
        // 申請内容
        public string? Note { get; set;}

        [Required]
        // 削除フラグ
        public bool IsDeleted { get; set; } = false;

    }
}

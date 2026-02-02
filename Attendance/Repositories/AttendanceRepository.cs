using Attendance.Data;
using Attendance.Models;

namespace Attendance.Repositories
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly AppDbContext _context;

        public AttendanceRepository(AppDbContext context)
        {
            _context = context;
        }

        // 打刻追加処理
        public void Add(AttendanceRecord record)
        {
            _context.AttendanceRecords.Add(record);
            _context.SaveChanges();
        }

        // 打刻検索(主キー検索)
        public AttendanceRecord? GetById(int id)
        {
            return _context.AttendanceRecords.Find(id); 
        }


        // 未退勤データの取得
        public AttendanceRecord? GetLastActiveRecord(string userId)
        {
            return _context.AttendanceRecords
                .Where(a => a.UserId == userId && a.ClockOutTime == null && !a.IsDeleted)
                .OrderByDescending(a => a.ClockInTime)
                .FirstOrDefault();
        }

        // 月次履歴取得 (履歴表示)
        public List<AttendanceRecord> GetMonthlyHistory(string userId, int year, int month)
        {
            return _context.AttendanceRecords
                .Where(a => a.UserId == userId 
                && a.WorkDate.Year == year 
                && a.WorkDate.Month == month
                && !a.IsDeleted)
                .OrderBy(a => a.WorkDate)
                .ToList();
        }

        // 今日の打刻をCheckする
        public bool HasClockInToday(string userId, DateTime date)
        {
            return _context.AttendanceRecords
                .Any(a => a.UserId == userId && a.WorkDate == date && !a.IsDeleted);
        }

        // 打刻を更新する
        public void Update(AttendanceRecord record)
        {
            _context.AttendanceRecords.Update(record);
            _context.SaveChanges();
        }
    }
}

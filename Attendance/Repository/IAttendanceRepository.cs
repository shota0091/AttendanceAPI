using Attendance.Models;

namespace MyAttendanceApi.Repositories
{
    public interface IAttendanceRepository
    {
        // 1. 今日の打刻データがあるかチェック (出勤時の二重打刻防止)
        bool HasClockInToday(string userId, DateTime date);

        // 2. データの保存 (出勤・打刻修正)
        void Add(AttendanceRecord record);

        // 3. データの更新 (退勤・論理削除・修正)
        void Update(AttendanceRecord record);

        // 4. 今日の最新の打刻データを取得 (退勤処理用)
        AttendanceRecord? GetLastActiveRecord(string userId);

        // 5. 月次データ取得 (履歴表示)
        List<AttendanceRecord> GetMonthlyHistory(string userId, int year, int month);

        // 6. ID指定で取得 (修正・削除用)
        AttendanceRecord? GetById(int id);
    }
}
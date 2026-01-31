using Attendance.DTOs;
using Attendance.Models;

namespace Attendance.Services
{
    // 打刻サービスのインターフェイス
    public interface IAttendanceService
    {
        // Web打刻：出勤
        AttendanceRecord ClockIn(UserRequest userRequest);

        // Web打刻：退勤
        AttendanceRecord ClockOut(UserRequest userRequest);

        // Web打刻：修正・削除
        AttendanceRecord UpdateAttendance(int id, AttendanceUpdateRequest aAttendanceUpdateRequest);

        // 月次情報の取得
        List<AttendanceRecord> GetMonthlyHistory(String userId, int year, int month);


    }
}

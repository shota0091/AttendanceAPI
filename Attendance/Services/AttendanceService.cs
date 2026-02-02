using Attendance.DTOs;
using Attendance.Models;
using Attendance.Repositories;

namespace Attendance.Services
{
    // Web打刻システム・実装クラス
    public class AttendanceService : IAttendanceService
    {
        private IAttendanceRepository _attendanceRepository;
        private readonly int _daySwitchHour;

        public AttendanceService(IAttendanceRepository attendanceRepository, IConfiguration config)
        {
            _attendanceRepository = attendanceRepository;
            _daySwitchHour = config.GetValue<int>("BusinessSettings:DaySwitchHour", 5);
        }

        // 打刻：出勤
        public AttendanceRecord ClockIn(UserRequest userRequest)
        {
            DateTime dt = DateTime.Now;
            DateTime workDate = (dt.Hour < _daySwitchHour) ? dt.Date.AddDays(-1) : dt.Date;
            if (_attendanceRepository.HasClockInToday(userRequest.UserId, workDate))
            {
                throw new InvalidOperationException("本日は既に出勤打刻済みです。");
            }
            AttendanceRecord record = new AttendanceRecord
            {
                UserId = userRequest.UserId,
                WorkDate = workDate,
                ClockInTime = dt,
                ApprovalStatus = 0
            };
            
            _attendanceRepository.Add(record);

            return record;
        }

        // 打刻：退勤
        public AttendanceRecord ClockOut(UserRequest userRequest)
        {
            AttendanceRecord? record = _attendanceRepository.GetLastActiveRecord(userRequest.UserId);
            if (record == null)
            {
                throw new InvalidOperationException("出勤データが見つかりません。先に打刻してください。");
            }
            DateTime dt = DateTime.Now;
            record.ClockOutTime = dt;
            _attendanceRepository.Update(record);

            return record;
        }

        // 月次勤怠の取得
        public List<AttendanceRecord> GetMonthlyHistory(string userId, int year, int month)
        {
            return _attendanceRepository.GetMonthlyHistory(userId, year, month);
        }

        // 更新
        public AttendanceRecord UpdateAttendance(int id, AttendanceUpdateRequest aAttendanceUpdateRequest)
        {
            AttendanceRecord? record = _attendanceRepository.GetById(id);
            if (record == null)
            {
                throw new KeyNotFoundException($"ID: {id} のデータが見つかりません。");
            }
            record.ClockInTime = aAttendanceUpdateRequest.ClockInTime;
            record.ClockOutTime = aAttendanceUpdateRequest.ClockOutTime;
            record.Note = aAttendanceUpdateRequest.Note;
            record.ApprovalStatus = 1;

            _attendanceRepository.Update(record);
            return  record;
        }
    }
}

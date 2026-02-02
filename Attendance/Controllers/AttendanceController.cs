using Microsoft.AspNetCore.Mvc;
using Attendance.DTOs;
using Attendance.Services;


namespace Attendance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _service;

        public AttendanceController(IAttendanceService service)
        {  
            _service = service;
        }

        // 勤怠：出勤
        [HttpPost("clock-in")]
        public IActionResult ClockIn([FromBody] UserRequest request)
        {
            try
            {
                var result = _service.ClockIn(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 勤怠：退勤
        [HttpPost("clock-out")]
        public IActionResult ClockOut([FromBody] UserRequest request)
        {
            try
            {
                var result = _service.ClockOut(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("history")]
        public IActionResult GetHistory([FromQuery] string userId, [FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                var result = _service.GetMonthlyHistory(userId, year, month);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] AttendanceUpdateRequest request)
        {
            try
            {
                var result = _service.UpdateAttendance(id, request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) // Serviceが投げた「見つからない」エラーを捕まえる
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex) // ロジックエラー（退勤済みなど）
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception) // その他想定外のエラー
            {
                // ログに出す処理など本来は必要
                return StatusCode(500, new { message = "予期せぬエラーが発生しました" });
            }
        }

    }
}

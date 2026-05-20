using Attendance.Application.Interfaces;
using Identity.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Attendance.API.Controllers
{
    [ApiController]
    [Route("api/attendance")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _service;

        public AttendanceController(IAttendanceService service)
        {
            _service = service;
        }

        [HttpPost("start")]
        [Authorize(Roles = UserRoles.Employee)]
        public async Task<IActionResult> StartShift()
        {
            var employeeId = int.Parse(User.FindFirst("EmployeeId")!.Value);
            var result = await _service.StartShiftAsync(employeeId);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(new { message = "تم تسجيل بداية العمل بنجاح" });
        }

        [HttpPost("end")]
        [Authorize(Roles = UserRoles.Employee)]
        public async Task<IActionResult> EndShift()
        {
            var employeeId = int.Parse(User.FindFirst("EmployeeId")!.Value);
            var result = await _service.EndShiftAsync(employeeId);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(new { message = "تم إنهاء الشيفت بنجاح" });
        }

        [HttpGet("reports")]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.Control},{UserRoles.Manager}")]
        public async Task<IActionResult> GetReport(
            [FromQuery] string type = "all",
            [FromQuery] DateOnly? fromDate = null,
            [FromQuery] DateOnly? toDate = null,
            [FromQuery] int? employeeId = null,
            [FromQuery] int? branchId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone));

            var from = fromDate ?? egyptNow;
            var to = toDate ?? egyptNow;

            var result = await _service.GetReportAsync(type, from, to, employeeId, branchId, page, pageSize);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(result.Value);
        }

        [HttpGet("reports/absent")]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.Control},{UserRoles.Manager}")]
        public async Task<IActionResult> GetAbsentReport(
            [FromQuery] DateOnly? fromDate = null,
            [FromQuery] DateOnly? toDate = null,
            [FromQuery] int? branchId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone));

            var from = fromDate ?? egyptNow;
            var to = toDate ?? egyptNow;

            var result = await _service.GetAbsentReportAsync(from, to, branchId, page, pageSize);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(result.Value);
        }

        [HttpGet("my-shifts")]
        [Authorize(Roles = UserRoles.Employee)]
        public async Task<IActionResult> GetMyShifts(
            [FromQuery] DateOnly? fromDate = null,
            [FromQuery] DateOnly? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var employeeId = int.Parse(User.FindFirst("EmployeeId")!.Value);

            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone));

            var from = fromDate ?? egyptNow;
            var to = toDate ?? egyptNow;

            var result = await _service.GetMyShiftsAsync(employeeId, from, to, page, pageSize);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(result.Value);
        }
    }
}
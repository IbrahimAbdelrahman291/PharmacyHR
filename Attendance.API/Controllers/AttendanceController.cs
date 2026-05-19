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

        [HttpGet("{employeeId}")]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.Manager},{UserRoles.Control}")]
        public async Task<IActionResult> GetAll(int employeeId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetAllAsync(employeeId, page, pageSize);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(result.Value);
        }

        [HttpGet("{employeeId}/open")]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.Manager},{UserRoles.Employee}")]
        public async Task<IActionResult> GetOpenShift(int employeeId)
        {
            var result = await _service.GetOpenShiftAsync(employeeId);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(result.Value);
        }
        [HttpGet("reports")]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.Control},{UserRoles.Manager}")]
        public async Task<IActionResult> GetReport(
            [FromQuery] DateOnly fromDate,
            [FromQuery] DateOnly toDate,
            [FromQuery] int? employeeId = null,
            [FromQuery] int? branchId = null)
        {
            var result = await _service.GetReportAsync(fromDate, toDate, employeeId, branchId);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(result.Value);
        }
    }
}
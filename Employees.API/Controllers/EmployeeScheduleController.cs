using Employees.Application.DTOs;
using Employees.Application.Interfaces;
using Identity.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Employees.API.Controllers
{
    [ApiController]
    [Route("api/employees/{employeeId}/schedule")]
    public class EmployeeScheduleController : ControllerBase
    {
        private readonly IEmployeeScheduleService _service;

        public EmployeeScheduleController(IEmployeeScheduleService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> AddSchedule(int employeeId, [FromBody] CreateEmployeeScheduleDto dto)
        {
            var result = await _service.AddScheduleAsync(employeeId, dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(new { message = "تم إضافة الشيفت بنجاح" });
        }

        [HttpPut("{scheduleId}")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> UpdateSchedule(int employeeId,int scheduleId, [FromBody] CreateEmployeeScheduleDto dto)
        {
            var result = await _service.UpdateScheduleAsync(employeeId,scheduleId, dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(new { message = "تم تعديل الجدول بنجاح" });
        }

        [HttpDelete("{scheduleId}")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> DeleteSchedule(int employeeId, int scheduleId)
        {
            var result = await _service.DeleteScheduleAsync(scheduleId);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(new { message = "تم حذف الجدول بنجاح" });
        }

        [HttpGet]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.Manager},{UserRoles.Employee}")]
        public async Task<IActionResult> GetSchedules(int employeeId)
        {
            var result = await _service.GetSchedulesByEmployeeIdAsync(employeeId);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(result.Value);
        }
    }
}
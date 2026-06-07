using Employees.Application.DTOs;
using Employees.Application.Interfaces;
using Identity.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Interfaces;
using System.Security.Claims;

namespace Employees.API.Controllers
{
    [ApiController]
    [Route("api/employees/{employeeId}/schedule")]
    public class EmployeeScheduleController : ControllerBase
    {
        private readonly IEmployeeScheduleService _service;
        private readonly IAduitService _aduitService;

        public EmployeeScheduleController(IEmployeeScheduleService service, SharedKernel.Interfaces.IAduitService aduitService)
        {
            _service = service;
            _aduitService = aduitService;
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

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;


            await _aduitService.LogDetailsAsync(userId, userName, $"تعديل جدول الحضور والانصراف الخاص بالموظف {employeeId}");

            return Ok(new { message = "تم تعديل الجدول بنجاح" });
        }

        [HttpDelete("{scheduleId}")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> DeleteSchedule(int employeeId, int scheduleId)
        {
            var result = await _service.DeleteScheduleAsync(scheduleId);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;


            await _aduitService.LogDetailsAsync(userId, userName, $"حذف يوم من جدول الحضور والانصراف الخاص بالموظف {employeeId}");

            return Ok(new { message = "تم حذف الجدول بنجاح" });
        }

        [HttpGet]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.AreaManager},{UserRoles.Employee}")]
        public async Task<IActionResult> GetSchedules(int employeeId)
        {
            var result = await _service.GetSchedulesByEmployeeIdAsync(employeeId);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(result.Value);
        }
    }
}
using System.Security.Claims;
using Identity.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Requests.Application.DTOs;
using Requests.Application.Interfaces;

namespace Requests.API.Controllers
{
    [ApiController]
    [Route("api/holidays")]
    public class HolidayController : ControllerBase
    {
        private readonly IHolidayService _service;
        private readonly SharedKernel.Interfaces.IAuditService _aduitService;

        public HolidayController(IHolidayService service, SharedKernel.Interfaces.IAuditService aduitService)
        {
            _service = service;
            _aduitService = aduitService;
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Employee)]
        public async Task<IActionResult> Add([FromBody] CreateHolidayDto dto)
        {
            var employeeId = int.Parse(User.FindFirst("EmployeeId")!.Value);
            var result = await _service.AddAsync(employeeId, dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(new { message = "تم إرسال طلب الإجازة بنجاح" });
        }

        [HttpGet]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.AreaManager},{UserRoles.Employee}")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? employeeId = null,
            [FromQuery] bool? isSeenByHR = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            if (User.IsInRole(UserRoles.Employee))
            {
                var empId = int.Parse(User.FindFirst("EmployeeId")!.Value);
                employeeId = empId;
            }

            var result = await _service.GetAllAsync(employeeId, isSeenByHR, userId, role, page, pageSize);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(result.Value);
        }

        [HttpPut("{id}/area-manager-approve")]
        [Authorize(Roles = UserRoles.AreaManager)]
        public async Task<IActionResult> AreaManagerApprove(int id, [FromBody] AreaManagerApproveHolidayDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _service.AreaManagerApproveAsync(id, userId, dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;


            await _aduitService.LogDetailsAsync(userId, userName, $"تمت الموافقة على طلب اجازة");

            return Ok(new { message = dto.IsApproved ? "تم قبول الطلب" : "تم رفض الطلب" });
        }

        [HttpPut("{id}/hr-approve")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> HRApprove(int id, [FromBody] HRApproveHolidayDto dto)
        {
            var result = await _service.HRApproveAsync(id, dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;


            await _aduitService.LogDetailsAsync(userId, userName, $"تمت الموافقة على طلب اجازة");

            return Ok(new { message = dto.IsApproved ? "تم قبول الطلب" : "تم رفض الطلب" });
        }

        [HttpGet("unseen-count")]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.Employee}")]
        public async Task<IActionResult> GetUnseenCount()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            var employeeId = User.IsInRole(UserRoles.Employee) ? int.Parse(User.FindFirst("EmployeeId")!.Value) : (int?)null;

            var result = await _service.GetUnseenCountAsync(role,employeeId);
            return Ok(new { count = result.Value });
        }

        [HttpPut("{id}/mark-seen")]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.Employee}")]
        public async Task<IActionResult> MarkAsSeen(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            var result = await _service.MarkAsSeenAsync(id,role);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(new { message = "تم تحديد الطلب كمقروء" });
        }
    }
}
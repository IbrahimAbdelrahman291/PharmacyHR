using System.Security.Claims;
using Identity.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Requests.Application.DTOs;
using Requests.Application.Interfaces;

namespace Requests.API.Controllers
{
    [ApiController]
    [Route("api/overtime")]
    public class OvertimeController : ControllerBase
    {
        private readonly IOvertimeService _service;

        public OvertimeController(IOvertimeService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Employee)]
        public async Task<IActionResult> Add([FromBody] CreateOvertimeRequestDto dto)
        {
            var employeeId = int.Parse(User.FindFirst("EmployeeId")!.Value);
            var result = await _service.AddAsync(employeeId, dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(new { message = "تم إرسال طلب الأوفر تايم بنجاح" });
        }

        [HttpGet]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.Control},{UserRoles.AreaManager},{UserRoles.Employee}")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? employeeId = null,
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

            var result = await _service.GetAllAsync(employeeId, userId, role, page, pageSize);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(result.Value);
        }

        [HttpPut("{id}/control-approve")]
        [Authorize(Roles = UserRoles.Control)]
        public async Task<IActionResult> ControlApprove(int id, [FromBody] ApproveRejectDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _service.ControlApproveAsync(id, userId, dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(new { message = dto.IsApproved ? "تم قبول الطلب" : "تم رفض الطلب" });
        }

        [HttpPut("{id}/area-manager-approve")]
        [Authorize(Roles = UserRoles.AreaManager)]
        public async Task<IActionResult> AreaManagerApprove(int id, [FromBody] ApproveRejectDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _service.AreaManagerApproveAsync(id, userId, dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(new { message = dto.IsApproved ? "تم قبول الطلب" : "تم رفض الطلب" });
        }

        [HttpPut("{id}/hr-approve")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> HRApprove(int id, [FromBody] ApproveRejectDto dto)
        {
            var result = await _service.HRApproveAsync(id, dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(new { message = dto.IsApproved ? "تم قبول الطلب" : "تم رفض الطلب" });
        }

        [HttpGet("unseen-count")]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.Control},{UserRoles.AreaManager}")]
        public async Task<IActionResult> GetUnseenCount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            var result = await _service.GetUnseenCountAsync(userId, role);
            return Ok(new { count = result.Value });
        }

        [HttpPut("{id}/mark-seen")]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.Control},{UserRoles.AreaManager}")]
        public async Task<IActionResult> MarkAsSeen(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            var result = await _service.MarkAsSeenAsync(id, role);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(new { message = "تم تحديد الطلب كمقروء" });
        }
    }
}
using System.Security.Claims;
using Identity.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Requests.Application.DTOs;
using Requests.Application.Interfaces;

namespace Requests.API.Controllers
{
    [ApiController]
    [Route("api/appointments")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _service;

        public AppointmentController(IAppointmentService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.AreaManager)]
        public async Task<IActionResult> Add([FromBody] CreateAppointmentRequestDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _service.AddAsync(userId, dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(new { message = "تم إرسال طلب التعيين بنجاح" });
        }

        [HttpGet]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> GetAll(
            [FromQuery] bool? isSeenByHR = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetAllAsync(isSeenByHR, page, pageSize);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(result.Value);
        }

        [HttpPut("{id}/approve-reject")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> ApproveOrReject(int id, [FromBody] ApproveRejectDto dto)
        {
            var result = await _service.ApproveOrRejectAsync(id, dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(new { message = dto.IsApproved ? "تم قبول الطلب وتم تعيين الموظف" : "تم رفض الطلب" });
        }

        [HttpGet("unseen-count")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> GetUnseenCount()
        {
            var result = await _service.GetUnseenCountAsync();
            return Ok(new { count = result.Value });
        }

        [HttpPut("{id}/mark-seen")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> MarkAsSeen(int id)
        {
            var result = await _service.MarkAsSeenAsync(id);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(new { message = "تم تحديد الطلب كمقروء" });
        }
    }
}
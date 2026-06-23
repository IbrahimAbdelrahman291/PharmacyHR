using Identity.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Requests.Application.DTOs;
using Requests.Application.Interfaces;
using SharedKernel.Interfaces;
using System.Security.Claims;

namespace Requests.API.Controllers
{
    [ApiController]
    [Route("api/complaints")]
    public class ComplaintController : ControllerBase
    {
        private readonly IComplaintService _service;
        private readonly IAuditService _aduitService;

        public ComplaintController(IComplaintService service, SharedKernel.Interfaces.IAuditService aduitService)
        {
            _service = service;
            _aduitService = aduitService;
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Employee)]
        public async Task<IActionResult> Add([FromBody] CreateComplaintDto dto)
        {
            var employeeId = int.Parse(User.FindFirst("EmployeeId")!.Value);
            var result = await _service.AddAsync(employeeId, dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(new { message = "تم إرسال الشكوى بنجاح" });
        }

        [HttpGet]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.AreaManager},{UserRoles.CEO},{UserRoles.Employee}")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? employeeId = null,
            [FromQuery] bool? isSeenByHR = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            var result = await _service.GetAllAsync(employeeId, isSeenByHR, userId, role, page, pageSize);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(result.Value);
        }

        [HttpPut("{id}/respond")]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.AreaManager},{UserRoles.CEO}")]
        public async Task<IActionResult> Respond(int id, [FromBody] RespondComplaintDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            var result = await _service.RespondAsync(id, dto, userId, role);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;


            await _aduitService.LogDetailsAsync(userId, userName, $"تم الرد على شكوى");


            return Ok(new { message = "تم الرد على الشكوى بنجاح" });
        }

        [HttpGet("unseen-count")]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.AreaManager},{UserRoles.CEO},{UserRoles.Employee}")]
        public async Task<IActionResult> GetUnseenCount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            var employeeId = User.IsInRole(UserRoles.Employee) ? int.Parse(User.FindFirst("EmployeeId")!.Value) : (int?)null;


            var result = await _service.GetUnseenCountAsync(userId, role,employeeId);
            return Ok(new { count = result.Value });
        }

        [HttpPut("{id}/mark-seen")]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.AreaManager},{UserRoles.CEO},{UserRoles.Employee}")]
        public async Task<IActionResult> MarkAsSeen(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            var result = await _service.MarkAsSeenAsync(id, userId, role);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(new { message = "تم تحديد الشكوى كمقروءة" });
        }
    }
}
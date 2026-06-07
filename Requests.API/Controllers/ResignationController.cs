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
    [Route("api/resignations")]
    public class ResignationController : ControllerBase
    {
        private readonly IResignationService _service;
        private readonly IAuditService _aduitService;

        public ResignationController(IResignationService service, SharedKernel.Interfaces.IAuditService aduitService)
        {
            _service = service;
            _aduitService = aduitService;
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Employee)]
        public async Task<IActionResult> Add([FromBody] CreateResignationDto dto)
        {
            var employeeId = int.Parse(User.FindFirst("EmployeeId")!.Value);
            var result = await _service.AddAsync(employeeId, dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(new { message = "تم إرسال طلب الاستقالة بنجاح" });
        }

        [HttpGet]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.Employee}")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? employeeId = null,
            [FromQuery] bool? isSeenByHR = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (User.IsInRole(UserRoles.Employee))
            {
                var empId = int.Parse(User.FindFirst("EmployeeId")!.Value);
                employeeId = empId;
            }

            var result = await _service.GetAllAsync(employeeId, isSeenByHR, page, pageSize);
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

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;


            await _aduitService.LogDetailsAsync(userId, userName, dto.IsApproved ? "تم الموافقة على طلب استقالة" : "تم رفض طلب استقالة");

            return Ok(new { message = dto.IsApproved ? "تم قبول الاستقالة" : "تم رفض الاستقالة" });
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
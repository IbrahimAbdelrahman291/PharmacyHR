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
    [Route("api/borrows")]
    public class BorrowController : ControllerBase
    {
        private readonly IBorrowService _service;
        private readonly IAduitService _aduitService;

        public BorrowController(IBorrowService service, SharedKernel.Interfaces.IAduitService aduitService)
        {
            _service = service;
            _aduitService = aduitService;
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Employee)]
        public async Task<IActionResult> AddBorrowRequest([FromBody] CreateBorrowRequestDto dto)
        {
            var employeeId = int.Parse(User.FindFirst("EmployeeId")!.Value);
            var result = await _service.AddBorrowRequestAsync(employeeId, dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(new { message = "تم إرسال طلب السلفة بنجاح" });
        }

        [HttpGet]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.Employee}")]
        public async Task<IActionResult> GetAllBorrowRequests(
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

            var result = await _service.GetAllBorrowRequestsAsync(employeeId, isSeenByHR, page, pageSize);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(result.Value);
        }

        [HttpPut("{id}/approve-reject")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> ApproveBorrowRequest(int id, [FromBody] ApproveRejectDto dto)
        {
            var result = await _service.ApproveBorrowRequestAsync(id, dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;


            await _aduitService.LogDetailsAsync(userId, userName, dto.IsApproved ? "تم قبول طلب سلفة" : "تم رفض طلب سلفة");


            return Ok(new { message = dto.IsApproved ? "تم قبول الطلب" : "تم رفض الطلب" });
        }

        [HttpGet("unseen-count")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> GetUnseenCount()
        {
            var result = await _service.GetUnseenBorrowCountAsync();
            return Ok(new { count = result.Value });
        }

        [HttpPut("{id}/mark-seen")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> MarkAsSeen(int id)
        {
            var result = await _service.MarkBorrowAsSeenAsync(id);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(new { message = "تم تحديد الطلب كمقروء" });
        }

        [HttpPost("installment")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> AddInstallmentBorrow([FromBody] CreateInstallmentBorrowDto dto)
        {
            var result = await _service.AddInstallmentBorrowAsync(dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;


            await _aduitService.LogDetailsAsync(userId, userName, $"تم اضافة سلفة مرحلة للموظف {dto.EmployeeId}");


            return Ok(new { message = "تم إضافة السلفة المرحلة بنجاح" });
        }

        [HttpGet("installment/{employeeId}")]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.Employee}")]
        public async Task<IActionResult> GetInstallmentBorrows(int employeeId)
        {
            var result = await _service.GetInstallmentBorrowsByEmployeeAsync(employeeId);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(result.Value);
        }
    }
}
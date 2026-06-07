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
    [Route("api/employees/{employeeId}/custody")]
    public class PersonalCustodyController : ControllerBase
    {
        private readonly IPersonalCustodyService _service;
        private readonly IAuditService _aduitService;

        public PersonalCustodyController(IPersonalCustodyService service, SharedKernel.Interfaces.IAuditService aduitService)
        {
            _service = service;
            _aduitService = aduitService;
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> Add(int employeeId, [FromBody] CreatePersonalCustodyDto dto)
        {
            var result = await _service.AddAsync(employeeId, dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;


            await _aduitService.LogDetailsAsync(userId, userName, $"تم اضافة عهدة للموظف {employeeId}");

            return Ok(new { message = "تم إضافة العهدة بنجاح" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> Delete(int employeeId, int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;


            await _aduitService.LogDetailsAsync(userId, userName, $"تم مسح عهدة للموظف {employeeId}");

            return Ok(new { message = "تم حذف العهدة بنجاح" });
        }

        [HttpGet]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.Employee}")]
        public async Task<IActionResult> GetAll(int employeeId)
        {
            var result = await _service.GetByEmployeeIdAsync(employeeId);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(result.Value);
        }
    }
}
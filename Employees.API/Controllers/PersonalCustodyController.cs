using Employees.Application.DTOs;
using Employees.Application.Interfaces;
using Identity.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Employees.API.Controllers
{
    [ApiController]
    [Route("api/employees/{employeeId}/custody")]
    public class PersonalCustodyController : ControllerBase
    {
        private readonly IPersonalCustodyService _service;

        public PersonalCustodyController(IPersonalCustodyService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> Add(int employeeId, [FromBody] CreatePersonalCustodyDto dto)
        {
            var result = await _service.AddAsync(employeeId, dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(new { message = "تم إضافة العهدة بنجاح" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> Delete(int employeeId, int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
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
using Employees.Application.DTOs;
using Employees.Application.Interfaces;
using Identity.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Employees.API.Controllers
{
    [ApiController]
    [Route("api/employees")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _service;

        public EmployeesController(IEmployeeService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
        {
            var result = await _service.CreateAsync(dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(new { message = "Employee created successfully" });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.Admin},{UserRoles.Manager},{UserRoles.Employee}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(result.Value);
        }

        [HttpGet]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.Admin},{UserRoles.Manager}")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? branchId = null)
        {
            var result = await _service.GetAllAsync(page, pageSize, branchId);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(result.Value);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(new { message = "Employee updated successfully" });
        }
        [HttpGet("{id}/history")]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.Admin},{UserRoles.Manager},{UserRoles.Employee}")]
        public async Task<IActionResult> GetHistory(int id)
        {
            var result = await _service.GetHistoryAsync(id);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(result.Value);
        }

        [HttpPut("{id}/end-of-service")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> UpdateEndOfService(int id, [FromBody] UpdateEndOfServiceDto dto)
        {
            var result = await _service.UpdateEndOfServiceAsync(id, dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(new { message = "End of service updated successfully" });
        }
    }
}

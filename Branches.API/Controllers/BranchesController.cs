using Branches.Application.DTOs;
using Branches.Application.Interfaces;
using Identity.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Branches.API.Controllers
{
    [ApiController]
    [Route("api/branches")]
    public class BranchesController : ControllerBase
    {
        private readonly IBranchService _service;

        public BranchesController(IBranchService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.Admin}")]
        public async Task<IActionResult> Add([FromBody] CreateBranchDto dto)
        {
            var result = await _service.AddAsync(dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(new { message = "Branch added successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.Admin}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(new { message = "Branch deleted successfully" });
        }

        [HttpGet]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Control},{UserRoles.HR},{UserRoles.AreaManager},{UserRoles.Accountant},{UserRoles.CEO}")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetAllAsync(page, pageSize);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(result.Value);
        }
    }
}
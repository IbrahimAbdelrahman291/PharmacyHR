using Employees.Application.DTOs;
using Employees.Application.Interfaces;
using Identity.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Employees.API.Controllers
{
    [ApiController]
    [Route("api/banks")]
    public class BanksController : ControllerBase
    {
        private readonly IBankService _service;

        public BanksController(IBankService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Add([FromBody] CreateBankDto dto)
        {
            var result = await _service.AddAsync(dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(new { message = "Bank added successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(new { message = "Bank deleted successfully" });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(result.Value);
        }
    }
}
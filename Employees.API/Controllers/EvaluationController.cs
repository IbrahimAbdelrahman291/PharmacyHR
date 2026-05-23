using Employees.Application.DTOs;
using Employees.Application.Interfaces;
using Identity.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Employees.API.Controllers
{
    [ApiController]
    [Route("api/evaluations")]
    public class EvaluationController : ControllerBase
    {
        private readonly IEvaluationService _service;

        public EvaluationController(IEvaluationService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.AreaManager)]
        public async Task<IActionResult> AddEvaluation([FromBody] CreateEvaluationDto dto)
        {
            var evaluatedBy = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
            var result = await _service.AddEvaluationAsync(dto, evaluatedBy);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(new { message = "تم إضافة التقييم بنجاح" });
        }

        [HttpGet("{employeeId}")]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.AreaManager}")]
        public async Task<IActionResult> GetEvaluations(int employeeId)
        {
            var result = await _service.GetEvaluationsAsync(employeeId);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(result.Value);
        }
    }
}
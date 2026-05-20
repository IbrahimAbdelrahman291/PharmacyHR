using Identity.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;

namespace Payroll.API.Controllers
{
    [ApiController]
    [Route("api/payroll")]
    public class PayrollController : ControllerBase
    {
        private readonly IPayrollService _service;

        public PayrollController(IPayrollService service)
        {
            _service = service;
        }

        [HttpGet("{employeeId}/current")]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.Admin},{UserRoles.Manager},{UserRoles.Employee}")]
        public async Task<IActionResult> GetCurrentMonth(int employeeId)
        {
            var result = await _service.GetCurrentMonthAsync(employeeId);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(result.Value);
        }

        [HttpGet("{employeeId}/{month}/{year}")]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.Admin},{UserRoles.Manager},{UserRoles.Employee}")]
        public async Task<IActionResult> GetByMonthAndYear(int employeeId, int month, int year)
        {
            var result = await _service.GetByMonthAndYearAsync(employeeId, month, year);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(result.Value);
        }

        [HttpPut("{employeeId}/monthly-data")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> UpdateMonthlyData(int employeeId, [FromBody] UpdateMonthlyDataDto dto)
        {
            var result = await _service.UpdateMonthlyDataAsync(employeeId, dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(new { message = "Monthly data updated successfully" });
        }

        [HttpPost("discount")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> AddDiscount([FromBody] AddDiscountDto dto)
        {
            var result = await _service.AddDiscountAsync(dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(new { message = "Discount added successfully" });
        }

        [HttpPost("contract-discount")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> AddContractDiscount([FromBody] AddDiscountDto dto)
        {
            var result = await _service.AddContractDiscountAsync(dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(new { message = "Contract discount added successfully" });
        }

        [HttpPost("bonus")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> AddBonus([FromBody] AddBonusDto dto)
        {
            var result = await _service.AddBonusAsync(dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(new { message = "Bonus added successfully" });
        }
        [HttpPost("cash-borrow")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> AddCashBorrow([FromBody] AddBorrowDto dto)
        {
            var result = await _service.AddCashBorrowAsync(dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(new { message = "Cash borrow added successfully" });
        }
        [HttpDelete("discount/{id}")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> DeleteDiscount(int id)
        {
            var result = await _service.DeleteDiscountAsync(id);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(new { message = "Discount deleted successfully" });
        }

        [HttpDelete("contract-discount/{id}")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> DeleteContractDiscount(int id)
        {
            var result = await _service.DeleteContractDiscountAsync(id);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(new { message = "Contract discount deleted successfully" });
        }

        [HttpDelete("bonus/{id}")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> DeleteBonus(int id)
        {
            var result = await _service.DeleteBonusAsync(id);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(new { message = "Bonus deleted successfully" });
        }

        [HttpDelete("cash-borrow/{id}")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> DeleteCashBorrow(int id)
        {
            var result = await _service.DeleteCashBorrowAsync(id);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(new { message = "Cash borrow deleted successfully" });
        }
        [HttpPost("discount/bulk")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> BulkDiscount([FromBody] BulkDiscountDto dto)
        {
            var result = await _service.BulkDiscountAsync(dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(new { message = "Bulk discount added successfully" });
        }

        [HttpPost("bonus/bulk")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> BulkBonus([FromBody] BulkBonusDto dto)
        {
            var result = await _service.BulkBonusAsync(dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(new { message = "Bulk bonus added successfully" });
        }
        [HttpGet("monthly-data")]
        [Authorize(Roles = UserRoles.Accountant)]
        public async Task<IActionResult> GetAllMonthlyData([FromQuery] int month,[FromQuery] int year,[FromQuery] int? branchId = null)
        {
            var result = await _service.GetAllMonthlyDataAsync(month, year, branchId);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(result.Value);
        }

        [HttpPost("deduction-calculator")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> CalculateDeductions([FromBody] DeductionCalculatorRequestDto dto)
        {
            var result = await _service.CalculateDeductionsAsync(dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(result.Value);
        }

        [HttpPost("discount/bulk-varied")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> BulkVariedDiscount([FromBody] IList<BulkVariedItemDto> items)
        {
            var result = await _service.BulkVariedDiscountAsync(items);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(new { message = "تم إضافة الخصومات بنجاح" });
        }

        [HttpPost("contract-discount/bulk-varied")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> BulkVariedContractDiscount([FromBody] IList<BulkVariedItemDto> items)
        {
            var result = await _service.BulkVariedContractDiscountAsync(items);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(new { message = "تم إضافة خصومات التعاقد بنجاح" });
        }

        [HttpPost("bonus/bulk-varied")]
        [Authorize(Roles = UserRoles.HR)]
        public async Task<IActionResult> BulkVariedBonus([FromBody] IList<BulkVariedItemDto> items)
        {
            var result = await _service.BulkVariedBonusAsync(items);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(new { message = "تم إضافة البونص بنجاح" });
        }
        [HttpGet("{employeeId}/details")]
        [Authorize(Roles = $"{UserRoles.HR},{UserRoles.Admin},{UserRoles.Accountant},{UserRoles.Employee}")]
        public async Task<IActionResult> GetDetails(int employeeId, [FromQuery] int? month = null, [FromQuery] int? year = null)
        {
            var result = await _service.GetDetailsAsync(employeeId, month, year);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(result.Value);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Payroll.Application.DTOs
{
    public class PayrollDetailsDto
    {
        public IList<DiscountItemDto> Discounts { get; set; } = new List<DiscountItemDto>();
        public IList<DiscountItemDto> ContractDiscounts { get; set; } = new List<DiscountItemDto>();
        public IList<BonusItemDto> Bonuses { get; set; } = new List<BonusItemDto>();
        public IList<BorrowItemDto> CashBorrows { get; set; } = new List<BorrowItemDto>();
    }
}

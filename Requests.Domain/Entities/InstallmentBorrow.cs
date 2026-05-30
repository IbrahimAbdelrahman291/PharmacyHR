using SharedKernel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Requests.Domain.Entities
{
    public class InstallmentBorrow : BaseEntity
    {
        public int EmployeeId { get; set; }
        public double TotalAmount { get; set; }
        public double MonthlyAmount { get; set; }
        public int TotalMonths { get; set; }
        public int RemainingMonths { get; set; }
        public int StartMonth { get; set; }
        public int StartYear { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public bool HasResignation { get; set; } = false; // تنبيه لو عنده استقالة
    }
}

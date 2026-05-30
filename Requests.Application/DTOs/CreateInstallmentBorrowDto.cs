using System;
using System.Collections.Generic;
using System.Text;

namespace Requests.Application.DTOs
{
    public class CreateInstallmentBorrowDto
    {
        public int EmployeeId { get; set; }
        public double TotalAmount { get; set; }
        public int TotalMonths { get; set; }
        public int StartMonth { get; set; }
        public int StartYear { get; set; }
    }
}

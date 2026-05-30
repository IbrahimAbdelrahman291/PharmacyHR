using System;
using System.Collections.Generic;
using System.Text;

namespace Requests.Application.DTOs
{
    public class CreateBorrowRequestDto
    {
        public double Amount { get; set; }
        public string? Notes { get; set; }
    }
}

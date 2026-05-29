using System;
using System.Collections.Generic;
using System.Text;

namespace Requests.Application.DTOs
{
    public class CreateHolidayDto
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
    }
}



namespace Requests.Application.DTOs
{
    public class CreateOvertimeRequestDto
    {
        public double Hours { get; set; }
        public string? Notes { get; set; }
        public DateTime? DateOfShift { get; set; }
    }
}

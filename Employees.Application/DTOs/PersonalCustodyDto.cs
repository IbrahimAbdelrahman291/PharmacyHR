

namespace Employees.Application.DTOs
{
    public class PersonalCustodyDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

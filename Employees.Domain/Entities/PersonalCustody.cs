using SharedKernel.Common;

namespace Employees.Domain.Entities
{
    public class PersonalCustody : BaseEntity
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
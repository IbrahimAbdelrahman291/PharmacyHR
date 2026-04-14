using System;
using System.Collections.Generic;
using System.Text;

namespace SharedKernel.Interfaces
{
    public interface ICurrentUser
    {
        string UserId { get; }
        string Name { get; }
        string Role { get; }
        int? EmployeeId { get; }
        int? BranchId { get; }
    }
}

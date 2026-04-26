using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Domain.Entities
{
    public class User : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public int? EmployeeId { get; set; }
        public int? BranchId { get; set; }
        public bool IsActive { get; set; } = true;

    }
}

using SharedKernel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Domain.Entities
{
    public class AreaManagerBranch : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public int BranchId { get; set; }
    }
}

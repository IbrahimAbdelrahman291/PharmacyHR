using SharedKernel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Branches.Domain.Entities
{
    public class Branch : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
    }
}

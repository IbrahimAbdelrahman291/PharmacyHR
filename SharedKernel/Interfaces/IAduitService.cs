using System;
using System.Collections.Generic;
using System.Text;

namespace SharedKernel.Interfaces
{
    public interface IAuditService
    {
        Task LogDetailsAsync(string userId, string userName, string action);

    }
}

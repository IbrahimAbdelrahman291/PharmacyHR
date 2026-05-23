using System;
using System.Collections.Generic;
using System.Text;

namespace SharedKernel.Interfaces
{
    public interface IAuthRepository
    {
        Task<bool> CreateUserAsync(string username, string password, string role, string name, int? employeeId, int? branchId);

    }
}

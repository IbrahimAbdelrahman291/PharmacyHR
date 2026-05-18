using Employees.Application.DTOs;
using SharedKernel.Wrappers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Application.Interfaces
{
    public interface IBankService
    {
        Task<Result<bool>> AddAsync(CreateBankDto dto);
        Task<Result<bool>> DeleteAsync(int id);
        Task<Result<IList<BankDto>>> GetAllAsync();
    }
}

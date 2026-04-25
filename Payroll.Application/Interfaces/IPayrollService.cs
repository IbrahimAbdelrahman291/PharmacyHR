using Payroll.Application.DTOs;
using SharedKernel.Wrappers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Payroll.Application.Interfaces
{
    public interface IPayrollService
    {
        Task<Result<MonthlyDataDto>> GetCurrentMonthAsync(int employeeId);
        Task<Result<MonthlyDataDto>> GetByMonthAndYearAsync(int employeeId, int month, int year);
        Task<Result<bool>> UpdateMonthlyDataAsync(int employeeId, UpdateMonthlyDataDto dto);
        Task<Result<bool>> AddDiscountAsync(AddDiscountDto dto);
        Task<Result<bool>> AddContractDiscountAsync(AddDiscountDto dto);
        Task<Result<bool>> AddBonusAsync(AddBonusDto dto);
        Task<Result<bool>> AddCashBorrowAsync(AddBorrowDto dto);

    }
}

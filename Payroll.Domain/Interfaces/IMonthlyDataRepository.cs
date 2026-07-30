using Payroll.Domain.Entities;
using SharedKernel.Wrappers;

namespace Payroll.Domain.Interfaces
{
    public interface IMonthlyDataRepository
    {
        Task<MonthlyEmployeeData?> GetCurrentMonthAsync(int employeeId);
        Task<MonthlyEmployeeData?> GetByMonthAndYearAsync(int employeeId, int month, int year);
        Task<bool> AddAsync(MonthlyEmployeeData data);
        Task<bool> UpdateAsync(MonthlyEmployeeData data);
        Task<IList<MonthlyEmployeeData>> GetAllCurrentMonthAsync(int? branchId);

        Task<Result<bool>> DeleteDiscountAsync(int id);
        Task<Result<bool>> DeleteContractDiscountAsync(int id);
        Task<Result<bool>> DeleteBonusAsync(int id);
        Task<Result<bool>> DeleteCashBorrowAsync(int id);

        Task<IList<MonthlyEmployeeData>> GetAllByMonthAndYearAsync(int month, int year, int? branchId, int page, int pageSize);
        Task<IList<Discount>> GetDiscountsAsync(int employeeId, int? month, int? year);
        Task<IList<ContractDiscount>> GetContractDiscountsAsync(int employeeId, int? month, int? year);
        Task<IList<Bonus>> GetBonusesAsync(int employeeId, int? month, int? year);
        Task<IList<CashBorrow>> GetCashBorrowsAsync(int employeeId, int? month, int? year);
        Task<int> GetTotalMonthlyDataCount(int? month, int? year, int? branchId);

    }
}

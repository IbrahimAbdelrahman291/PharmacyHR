using SharedKernel.Wrappers;

namespace SharedKernel.Interfaces
{
    public interface IMonthlyDataRepository
    {
        Task<double?> GetTotalSalaryForInstallmentBorrow(int employeeId);
        Task<int?> GetHolidaysInCurrentMonthAsync(int employeeId);

        Task<Result<bool>> AddHoursAsync(int employeeId, double hours);
        Task<Result<bool>> AddHolidayHoursAsync(int employeeId, double hours, int TotalDays);
        Task<Result<bool>> AddForgetedHoursAsync(int employeeId, double hours);
        Task<Result<bool>> UpdateHoursOverTimeAsync(int employeeId, double hours);
        Task<Result<bool>> AddDiscountAsync(int employeeId, double amount, string reason, string? notes);
        Task<Result<bool>> AddContractDiscountAsync(int employeeId, double amount, string reason, string? notes);
        Task<Result<bool>> AddBonusAsync(int employeeId, double amount, string reason, string? notes);
        Task<Result<bool>> AddBorrowAsync(int employeeId, double amount);
        Task<Result<bool>> UpdateInstallmentBorrow(int employeeId, double amount);
        Task<Result<bool>> AddCashBorrowAsync(int employeeId, double amount, string reason, string? notes);
        Task<Result<bool>> UpdateSalaryAsync(int employeeId, double totalSalary);
        Task<Result<bool>> UpdateSalaryPerHourAsync(int employeeId, double salaryPerHour);
        Task<Result<bool>> UpdateInsurenceAsync(int employeeId, double amount);
        Task<Result<bool>> UpdateHolidays(int employeeId, int Holidays);
        Task<Result<bool>> UpdateHolidaysHours(int employeeId, double HolidayHours);

        Task<Result<bool>> DeleteDiscountAsync(int id);
        Task<Result<bool>> DeleteContractDiscountAsync(int id);
        Task<Result<bool>> DeleteBonusAsync(int id);
        Task<Result<bool>> DeleteCashBorrowAsync(int id);

        Task CreateMonthlyDataAsync(int employeeId, string role, double? totalSalary, double? salaryPerHour, double target, int branchId, double? insurence, int Holidaies);

        Task<Result<bool>> BulkAddDiscountAsync(IList<int> employeeIds, double amount, string reason, string? notes);
        Task<Result<bool>> BulkAddBonusAsync(IList<int> employeeIds, double amount, string reason, string? notes);
    }
}



namespace SharedKernel.Interfaces
{
    public interface IMonthlyDataRepository
    {
        Task<double?> GetTotalSalaryForInstallmentBorrow(int employeeId);
        Task<int?> GetHolidaysInCurrentMonthAsync(int employeeId);
        Task AddHoursAsync(int employeeId, double hours);
        Task AddHolidayHoursAsync(int employeeId, double hours, int TotalDays);
        Task AddForgetedHoursAsync(int employeeId, double hours);
        Task UpdateHoursOverTimeAsync(int employeeId, double hours);
        Task AddDiscountAsync(int employeeId, double amount, string reason, string? notes);
        Task AddContractDiscountAsync(int employeeId, double amount, string reason, string? notes);
        Task AddBonusAsync(int employeeId, double amount, string reason, string? notes);
        Task AddBorrowAsync(int employeeId, double amount);
        Task AddCashBorrowAsync(int employeeId, double amount, string reason, string? notes);
        Task UpdateSalaryAsync(int employeeId, double totalSalary);
        Task UpdateSalaryPerHourAsync(int employeeId, double salaryPerHour);
        Task UpdateInsurenceAsync(int employeeId, double amount);
        Task<bool> DeleteDiscountAsync(int id);
        Task<bool> DeleteContractDiscountAsync(int id);
        Task<bool> DeleteBonusAsync(int id);
        Task<bool> DeleteCashBorrowAsync(int id);
        Task CreateMonthlyDataAsync(int employeeId, string role, double? totalSalary, double? salaryPerHour, double target, int branchId,double? insurence, int Holidaies);
        Task BulkAddDiscountAsync(IList<int> employeeIds, double amount, string reason, string? notes);
        Task BulkAddBonusAsync(IList<int> employeeIds, double amount, string reason, string? notes);
    }
}
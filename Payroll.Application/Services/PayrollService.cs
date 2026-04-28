using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Domain.Interfaces;
using SharedKernel.Wrappers;
using SharedKernel.Interfaces;

namespace Payroll.Application.Services
{
    public class PayrollService : IPayrollService
    {
        private readonly Payroll.Domain.Interfaces.IMonthlyDataRepository _repository;
        private readonly SharedKernel.Interfaces.IMonthlyDataRepository _sharedRepository;
        private readonly SharedKernel.Interfaces.IEmployeeRepository _employeeRepository;
        private readonly SharedKernel.Interfaces.IBranchRepository _branchRepository;

        public PayrollService(
            Payroll.Domain.Interfaces.IMonthlyDataRepository repository,
            SharedKernel.Interfaces.IMonthlyDataRepository sharedRepository,
            SharedKernel.Interfaces.IEmployeeRepository employeeRepository,
            SharedKernel.Interfaces.IBranchRepository branchRepository)
        {
            _repository = repository;
            _sharedRepository = sharedRepository;
            _employeeRepository = employeeRepository;
            _branchRepository = branchRepository;
        }

        public async Task<Result<MonthlyDataDto>> GetCurrentMonthAsync(int employeeId)
        {
            var data = await _repository.GetCurrentMonthAsync(employeeId);
            if (data is null)
                return Result<MonthlyDataDto>.Failure("Monthly data not found");

            return Result<MonthlyDataDto>.Success(MapToDto(data));
        }

        public async Task<Result<MonthlyDataDto>> GetByMonthAndYearAsync(int employeeId, int month, int year)
        {
            var data = await _repository.GetByMonthAndYearAsync(employeeId, month, year);
            if (data is null)
                return Result<MonthlyDataDto>.Failure("Monthly data not found");

            return Result<MonthlyDataDto>.Success(MapToDto(data));
        }

        public async Task<Result<bool>> UpdateMonthlyDataAsync(int employeeId, UpdateMonthlyDataDto dto)
        {
            var data = await _repository.GetCurrentMonthAsync(employeeId);
            if (data is null)
                return Result<bool>.Failure("Monthly data not found");

            if (dto.TotalSalary.HasValue)
                await _sharedRepository.UpdateSalaryAsync(employeeId, dto.TotalSalary.Value);

            if (dto.SalaryPerHour.HasValue)
                await _sharedRepository.UpdateSalaryPerHourAsync(employeeId, dto.SalaryPerHour.Value);

            if (dto.Insurence.HasValue)
                await _sharedRepository.UpdateInsurenceAsync(employeeId, dto.Insurence.Value);

            if (dto.HoursOverTime.HasValue)
                await _sharedRepository.UpdateHoursOverTimeAsync(employeeId, dto.HoursOverTime.Value);

            if (dto.ForgetedHours.HasValue)
                await _sharedRepository.AddForgetedHoursAsync(employeeId, dto.ForgetedHours.Value);

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> AddDiscountAsync(AddDiscountDto dto)
        {
            await _sharedRepository.AddDiscountAsync(dto.EmployeeId, dto.Amount, dto.ReasonOfDiscount, dto.Notes);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> AddContractDiscountAsync(AddDiscountDto dto)
        {
            await _sharedRepository.AddContractDiscountAsync(dto.EmployeeId, dto.Amount, dto.ReasonOfDiscount, dto.Notes);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> AddBonusAsync(AddBonusDto dto)
        {
            await _sharedRepository.AddBonusAsync(dto.EmployeeId, dto.Amount, dto.Reason, dto.Notes);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> AddCashBorrowAsync(AddBorrowDto dto)
        {
            await _sharedRepository.AddCashBorrowAsync(dto.EmployeeId, dto.Amount, dto.Reason, dto.Notes);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeleteDiscountAsync(int id)
        {
            var result = await _sharedRepository.DeleteDiscountAsync(id);
            if (!result)
                return Result<bool>.Failure("Discount not found");
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeleteContractDiscountAsync(int id)
        {
            var result = await _sharedRepository.DeleteContractDiscountAsync(id);
            if (!result)
                return Result<bool>.Failure("Contract discount not found");
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeleteBonusAsync(int id)
        {
            var result = await _sharedRepository.DeleteBonusAsync(id);
            if (!result)
                return Result<bool>.Failure("Bonus not found");
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeleteCashBorrowAsync(int id)
        {
            var result = await _sharedRepository.DeleteCashBorrowAsync(id);
            if (!result)
                return Result<bool>.Failure("Cash borrow not found");
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> BulkDiscountAsync(BulkDiscountDto dto)
        {
            await _sharedRepository.BulkAddDiscountAsync(dto.EmployeeIds, dto.Amount, dto.ReasonOfDiscount, dto.Notes);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> BulkBonusAsync(BulkBonusDto dto)
        {
            await _sharedRepository.BulkAddBonusAsync(dto.EmployeeIds, dto.Amount, dto.Reason, dto.Notes);
            return Result<bool>.Success(true);
        }

        public async Task<Result<IList<MonthlyDataWithEmployeeDto>>> GetAllMonthlyDataAsync(int month, int year, int? branchId)
        {
            var allData = await _repository.GetAllByMonthAndYearAsync(month, year, branchId);

            var result = new List<MonthlyDataWithEmployeeDto>();
            foreach (var data in allData)
            {
                var employeeInfo = await _employeeRepository.GetEmployeeBasicInfoAsync(data.EmployeeId);
                var branchInfo = await _branchRepository.GetBranchByIdAsync(data.BranchId);

                result.Add(new MonthlyDataWithEmployeeDto
                {
                    EmployeeId = data.EmployeeId,
                    EmployeeName = employeeInfo?.Name ?? string.Empty,
                    BranchId = data.BranchId,
                    BranchName = branchInfo?.Name ?? string.Empty,
                    BankName = employeeInfo?.BankName,
                    BankAccount = employeeInfo?.BankAccount,
                    Month = data.Month,
                    Year = data.Year,
                    TotalSalary = data.TotalSalary,
                    TotalDiscounts = data.TotalDiscounts,
                    TotalContractDiscount = data.TotalContractDiscount,
                    TotalBouns = data.TotalBouns,
                    TotalBorrows = data.TotalBorrows,
                    TotalCashBorrows = data.TotalCashBorrows,
                    NetSalary = data.NetSalary
                });
            }

            return Result<IList<MonthlyDataWithEmployeeDto>>.Success(result);
        }

        private MonthlyDataDto MapToDto(Payroll.Domain.Entities.MonthlyEmployeeData data) => new()
        {
            EmployeeId = data.EmployeeId,
            Month = data.Month,
            Year = data.Year,
            Hours = data.Hours,
            HoursOverTime = data.HoursOverTime,
            ForgetedHours = data.ForgetedHours,
            Target = data.Target,
            Insurence = data.Insurence,
            HolidayHours = data.HolidayHours,
            SalaryPerHour = data.SalaryPerHour,
            TotalSalary = data.TotalSalary,
            TotalDiscounts = data.TotalDiscounts,
            TotalContractDiscount = data.TotalContractDiscount,
            TotalBouns = data.TotalBouns,
            TotalBorrows = data.TotalBorrows,
            TotalCashBorrows = data.TotalCashBorrows,
            Holidaies = data.Holidaies,
            NetSalary = data.NetSalary,
            Discounts = data.Discounts.Select(d => new DiscountItemDto
            {
                Id = d.Id,
                Amount = d.Amount,
                ReasonOfDiscount = d.ReasonOfDiscount,
                Notes = d.Notes,
                Date = d.Date
            }).ToList(),
            ContractDiscounts = data.ContractDiscounts.Select(d => new DiscountItemDto
            {
                Id = d.Id,
                Amount = d.Amount,
                ReasonOfDiscount = d.ReasonOfDiscount,
                Notes = d.Notes,
                Date = d.Date
            }).ToList(),
            Bonuses = data.Bonuses.Select(b => new BonusItemDto
            {
                Id = b.Id,
                Amount = b.Amount,
                Reason = b.Reason,
                Notes = b.Notes,
                DateOfBonus = b.DateOfBonus
            }).ToList(),
            CashBorrows = data.CashBorrows.Select(c => new BorrowItemDto
            {
                Id = c.Id,
                Amount = c.Amount,
                Reason = c.Reason,
                Notes = c.Notes,
                DateOfBorrow = c.DateOfBorrow
            }).ToList()
        };
    }
}
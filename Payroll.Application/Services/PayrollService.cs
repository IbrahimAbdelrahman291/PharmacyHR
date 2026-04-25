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

        public PayrollService(
            Payroll.Domain.Interfaces.IMonthlyDataRepository repository,
            SharedKernel.Interfaces.IMonthlyDataRepository sharedRepository)
        {
            _repository = repository;
            _sharedRepository = sharedRepository;
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
            await _sharedRepository.AddDiscountAsync(dto.EmployeeId, dto.Amount);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> AddContractDiscountAsync(AddDiscountDto dto)
        {
            await _sharedRepository.AddContractDiscountAsync(dto.EmployeeId, dto.Amount);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> AddBonusAsync(AddBonusDto dto)
        {
            await _sharedRepository.AddBonusAsync(dto.EmployeeId, dto.Amount);
            return Result<bool>.Success(true);
        }
        public async Task<Result<bool>> AddCashBorrowAsync(AddBorrowDto dto)
        {
            await _sharedRepository.AddCashBorrowAsync(dto.EmployeeId, dto.Amount);
            return Result<bool>.Success(true);
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
            NetSalary = data.NetSalary
        };
    }
}
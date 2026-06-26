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

        public async Task<Result<PaginatedResponse<MonthlyDataWithEmployeeDto>>> GetAllMonthlyDataAsync(int month, int year, int? branchId,int page, int pageSize)
        {
            var allData = await _repository.GetAllByMonthAndYearAsync(month, year, branchId, page, pageSize);
            var totalCount = await _repository.GetTotalMonthlyDataCount(month,year,branchId);
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
                    Target = data.Target,
                    Insurence = data.Insurence,
                    Hours = data.Hours,
                    HoursOverTime = data.HoursOverTime,
                    ForgetedHours = data.ForgetedHours,
                    HolidayHours = data.HolidayHours,
                    TotalSalary = data.TotalSalary,
                    TotalDiscounts = data.TotalDiscounts,
                    TotalContractDiscount = data.TotalContractDiscount,
                    TotalBouns = data.TotalBouns,
                    TotalBorrows = data.TotalBorrows,
                    TotalCashBorrows = data.TotalCashBorrows,
                    NetSalary = data.NetSalary
                });
            }

            return Result<PaginatedResponse<MonthlyDataWithEmployeeDto>>.Success(new PaginatedResponse<MonthlyDataWithEmployeeDto> 
            {
                Data = result,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }
        public async Task<Result<IList<DeductionCalculatorResponseDto>>> CalculateDeductionsAsync(DeductionCalculatorRequestDto dto)
        {
            var result = new List<DeductionCalculatorResponseDto>();

            foreach (var employeeId in dto.EmployeeIds)
            {
                var employeeInfo = await _employeeRepository.GetEmployeeBasicInfoAsync(employeeId);
                if (employeeInfo is null) continue;

                var monthlyData = await _repository.GetCurrentMonthAsync(employeeId);
                if (monthlyData is null) continue;

                double oneDay = 0;

                if (monthlyData.Role == "static")
                {
                    oneDay = (monthlyData.TotalSalary ?? 0) / 26;
                }
                else if (monthlyData.Role == "changable")
                {
                    var shiftHours = await _employeeRepository.GetShiftHoursAsync(employeeId);
                    oneDay = (shiftHours ?? 0) * (monthlyData.SalaryPerHour ?? 0) / 26;
                }
                else if (monthlyData.Role == "delivery")
                {
                    var shiftHours = await _employeeRepository.GetShiftHoursAsync(employeeId);
                    oneDay = (shiftHours ?? 0) * (monthlyData.SalaryPerHour ?? 0);
                }

                result.Add(new DeductionCalculatorResponseDto
                {
                    EmployeeId = employeeId,
                    EmployeeName = employeeInfo.Value.Name,
                    Deductions = new DeductionValuesDto
                    {
                        QuarterDay = oneDay / 4,
                        HalfDay = oneDay / 2,
                        OneDay = oneDay,
                        TwoDays = oneDay * 2,
                        ThreeDays = oneDay * 3,
                        FiveDays = oneDay * 5,
                        TenDays = oneDay * 10
                    }
                });
            }

            return Result<IList<DeductionCalculatorResponseDto>>.Success(result);
        }

        public async Task<Result<bool>> BulkVariedDiscountAsync(IList<BulkVariedItemDto> items)
        {
            foreach (var item in items)
                await _sharedRepository.AddDiscountAsync(item.EmployeeId, item.Amount, item.Reason, item.Notes);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> BulkVariedContractDiscountAsync(IList<BulkVariedItemDto> items)
        {
            foreach (var item in items)
                await _sharedRepository.AddContractDiscountAsync(item.EmployeeId, item.Amount, item.Reason, item.Notes);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> BulkVariedBonusAsync(IList<BulkVariedItemDto> items)
        {
            foreach (var item in items)
                await _sharedRepository.AddBonusAsync(item.EmployeeId, item.Amount, item.Reason, item.Notes);
            return Result<bool>.Success(true);
        }
        public async Task<Result<PayrollDetailsDto>> GetDetailsAsync(int employeeId, int? month, int? year)
        {
            var discounts = await _repository.GetDiscountsAsync(employeeId, month, year);
            var contractDiscounts = await _repository.GetContractDiscountsAsync(employeeId, month, year);
            var bonuses = await _repository.GetBonusesAsync(employeeId, month, year);
            var cashBorrows = await _repository.GetCashBorrowsAsync(employeeId, month, year);

            return Result<PayrollDetailsDto>.Success(new PayrollDetailsDto
            {
                Discounts = discounts.Select(d => new DiscountItemDto
                {
                    Id = d.Id,
                    Amount = d.Amount,
                    ReasonOfDiscount = d.ReasonOfDiscount,
                    Notes = d.Notes,
                    Date = d.Date
                }).ToList(),
                ContractDiscounts = contractDiscounts.Select(d => new DiscountItemDto
                {
                    Id = d.Id,
                    Amount = d.Amount,
                    ReasonOfDiscount = d.ReasonOfDiscount,
                    Notes = d.Notes,
                    Date = d.Date
                }).ToList(),
                Bonuses = bonuses.Select(b => new BonusItemDto
                {
                    Id = b.Id,
                    Amount = b.Amount,
                    Reason = b.Reason,
                    Notes = b.Notes,
                    DateOfBonus = b.DateOfBonus
                }).ToList(),
                CashBorrows = cashBorrows.Select(c => new BorrowItemDto
                {
                    Id = c.Id,
                    Amount = c.Amount,
                    Reason = c.Reason,
                    Notes = c.Notes,
                    DateOfBorrow = c.DateOfBorrow
                }).ToList()
            });
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
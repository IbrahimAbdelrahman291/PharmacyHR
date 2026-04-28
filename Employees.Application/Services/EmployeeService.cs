using Employees.Application.DTOs;
using Employees.Application.Interfaces;
using Employees.Domain.Entities;
using Employees.Domain.Interfaces;
using SharedKernel.Interfaces;
using SharedKernel.Wrappers;

namespace Employees.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly Domain.Interfaces.IEmployeeRepository _employeeRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly IAuthRepository _authRepository;

        private readonly SharedKernel.Interfaces.IMonthlyDataRepository _monthlyDataRepository;

        public EmployeeService(
            Domain.Interfaces.IEmployeeRepository employeeRepository,
            IBranchRepository branchRepository,
            IAuthRepository authRepository,
            SharedKernel.Interfaces.IMonthlyDataRepository monthlyDataRepository)
        {
            _employeeRepository = employeeRepository;
            _branchRepository = branchRepository;
            _authRepository = authRepository;
            _monthlyDataRepository = monthlyDataRepository;
        }

        public async Task<Result<bool>> CreateAsync(CreateEmployeeDto dto)
        {
            var validRoles = new[] { "static", "changable", "delivery" };
            if (!validRoles.Contains(dto.Role.ToLower()))
                return Result<bool>.Failure("Invalid employee role");

            var branch = await _branchRepository.GetBranchByIdAsync(dto.BranchId);
            if (branch is null)
                return Result<bool>.Failure("Branch not found");

            var employee = new Employee
            {
                Name = dto.Name,
                Role = dto.Role.ToLower(),
                theNameOfJob = dto.theNameOfJob,
                BankName = dto.BankName,
                BankAccount = dto.BankAccount,
                CheckInTime = dto.CheckInTime,
                CheckOutTime = dto.CheckOutTime,
                ShiftHours = dto.ShiftHours,
                BranchId = dto.BranchId,
                UserId = string.Empty
            };

            var history = new EmployeeHistory
            {
                HiringDate = dto.HiringDate,
                Qualification = dto.Qualification,
                GraduationYear = dto.GraduationYear,
                NationalId = dto.NationalId,
                PhoneNumber = dto.PhoneNumber
            };

            await _employeeRepository.AddAsync(employee, history);

            var userCreated = await _authRepository.CreateUserAsync(
                dto.Username,
                dto.Password,
                "Employee",
                dto.Name,
                employee.Id,
                dto.BranchId
            );
            var target = (dto.ShiftHours ?? 0) * 26;
            await _monthlyDataRepository.CreateMonthlyDataAsync(
                employee.Id,
                dto.Role,
                dto.TotalSalary,
                dto.SalaryPerHour,
                target,
                dto.BranchId
            );
            if (!userCreated)
                return Result<bool>.Failure("Failed to create user");

            return Result<bool>.Success(true);
        }

        public async Task<Result<EmployeeDto>> GetByIdAsync(int id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee is null)
                return Result<EmployeeDto>.Failure("Employee not found");

            var branch = await _branchRepository.GetBranchByIdAsync(employee.BranchId);

            return Result<EmployeeDto>.Success(new EmployeeDto
            {
                Id = employee.Id,
                Name = employee.Name,
                Role = employee.Role,
                theNameOfJob = employee.theNameOfJob,
                BankName = employee.BankName,
                BankAccount = employee.BankAccount,
                CheckInTime = employee.CheckInTime,
                CheckOutTime = employee.CheckOutTime,
                ShiftHours = employee.ShiftHours,
                BranchId = employee.BranchId,
                BranchName = branch?.Name ?? string.Empty
            });
        }

        public async Task<Result<PaginatedResponse<EmployeeDto>>> GetAllAsync(int page, int pageSize, int? branchId)
        {
            var employees = await _employeeRepository.GetAllAsync(page, pageSize, branchId);
            var totalCount = await _employeeRepository.GetTotalCountAsync(branchId);

            var dtos = new List<EmployeeDto>();
            foreach (var employee in employees)
            {
                var branch = await _branchRepository.GetBranchByIdAsync(employee.BranchId);
                dtos.Add(new EmployeeDto
                {
                    Id = employee.Id,
                    Name = employee.Name,
                    Role = employee.Role,
                    theNameOfJob = employee.theNameOfJob,
                    BankName = employee.BankName,
                    BankAccount = employee.BankAccount,
                    CheckInTime = employee.CheckInTime,
                    CheckOutTime = employee.CheckOutTime,
                    ShiftHours = employee.ShiftHours,
                    BranchId = employee.BranchId,
                    BranchName = branch?.Name ?? string.Empty
                });
            }

            return Result<PaginatedResponse<EmployeeDto>>.Success(new PaginatedResponse<EmployeeDto>
            {
                Data = dtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        public async Task<Result<bool>> UpdateAsync(int id, UpdateEmployeeDto dto)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee is null)
                return Result<bool>.Failure("Employee not found");

            if (dto.theNameOfJob is not null) employee.theNameOfJob = dto.theNameOfJob;
            if (dto.BankName is not null) employee.BankName = dto.BankName;
            if (dto.BankAccount is not null) employee.BankAccount = dto.BankAccount;
            if (dto.CheckInTime.HasValue) employee.CheckInTime = dto.CheckInTime;
            if (dto.CheckOutTime.HasValue) employee.CheckOutTime = dto.CheckOutTime;
            if (dto.ShiftHours.HasValue) employee.ShiftHours = dto.ShiftHours;
            if (dto.BranchId.HasValue) employee.BranchId = dto.BranchId.Value;

            await _employeeRepository.UpdateAsync(employee);
            return Result<bool>.Success(true);
        }
        public async Task<Result<EmployeeHistoryDto>> GetHistoryAsync(int employeeId)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee is null)
                return Result<EmployeeHistoryDto>.Failure("Employee not found");

            var history = await _employeeRepository.GetHistoryByEmployeeIdAsync(employeeId);
            if (history is null)
                return Result<EmployeeHistoryDto>.Failure("Employee history not found");

            return Result<EmployeeHistoryDto>.Success(new EmployeeHistoryDto
            {
                EmployeeId = history.EmployeeId,
                HiringDate = history.HiringDate,
                Qualification = history.Qualification,
                GraduationYear = history.GraduationYear,
                NationalId = history.NationalId,
                PhoneNumber = history.PhoneNumber,
                EndOfServiceDate = history.EndOfServiceDate,
                EndOfServiceReason = history.EndOfServiceReason,
                EndOfServiceType = history.EndOfServiceType
            });
        }

        public async Task<Result<bool>> UpdateEndOfServiceAsync(int employeeId, UpdateEndOfServiceDto dto)
        {
            var history = await _employeeRepository.GetHistoryByEmployeeIdAsync(employeeId);
            if (history is null)
                return Result<bool>.Failure("Employee history not found");

            history.EndOfServiceDate = dto.EndOfServiceDate;
            history.EndOfServiceReason = dto.EndOfServiceReason;
            history.EndOfServiceType = dto.EndOfServiceType;

            await _employeeRepository.UpdateHistoryAsync(history);
            return Result<bool>.Success(true);
        }

    }
}
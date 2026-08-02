using Employees.Application.DTOs;
using Employees.Application.Interfaces;
using Employees.Domain.Entities;
using Employees.Domain.Interfaces;
using SharedKernel.Interfaces;
using SharedKernel.Wrappers;
using System.Security.Claims;
using System.Text.Json;

namespace Employees.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly Domain.Interfaces.IEmployeeRepository _employeeRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly IAuthRepository _authRepository;

        private readonly SharedKernel.Interfaces.IMonthlyDataRepository _monthlyDataRepository;
        private readonly IAuditService _auditService;

        public EmployeeService(
            Domain.Interfaces.IEmployeeRepository employeeRepository,
            IBranchRepository branchRepository,
            IAuthRepository authRepository,
            SharedKernel.Interfaces.IMonthlyDataRepository monthlyDataRepository,
            SharedKernel.Interfaces.IAuditService auditService
            )
        {
            _employeeRepository = employeeRepository;
            _branchRepository = branchRepository;
            _authRepository = authRepository;
            _monthlyDataRepository = monthlyDataRepository;
            _auditService = auditService;
        }

        public async Task<Result<bool>> CreateAsync(CreateEmployeeDto dto)
        {
            var validRoles = new[] { "static", "changable", "delivery" };
            if (!validRoles.Contains(dto.Role.ToLower()))
                return Result<bool>.Failure("Invalid employee role");

            var branch = await _branchRepository.GetBranchByIdAsync(dto.BranchId);
            if (branch is null)
                return Result<bool>.Failure("Branch not found");
            var excetsEmployee = _authRepository.FindByUsername(dto.Username);
            if (excetsEmployee.Result == true)
            {
                return Result<bool>.Failure("Failed to create user");
            }

            var employee = new Employee
            {
                Name = dto.Name,
                Role = dto.Role.ToLower(),
                theNameOfJob = dto.theNameOfJob,
                BankId = dto.BankId,
                BankAccount = dto.BankAccount,
                ShiftHours = dto.ShiftHours,
                IsHaveNightShift = false,
                BranchId = dto.BranchId,
                UserId = string.Empty,
                EmployeeType = dto.EmployeeType,
                Status = "Active"
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
            if (!userCreated)
                return Result<bool>.Failure("Failed to create user");

            var target = (dto.ShiftHours ?? 0) * 26;
            await _monthlyDataRepository.CreateMonthlyDataAsync(
                employee.Id,
                dto.Role,
                dto.TotalSalary,
                dto.SalaryPerHour,
                target,
                dto.BranchId,
                dto.Insurence,
                dto.Holidaies
            );
            await _employeeRepository.AddEmployeeBranchAsync(new EmployeeBranch
            {
                EmployeeId = employee.Id,
                BranchId = dto.BranchId,
                StartDate = DateTime.UtcNow
            });
            return Result<bool>.Success(true);
        }

        public async Task<Result<EmployeeDto>> GetByIdAsync(int id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee is null)
                return Result<EmployeeDto>.Failure("Employee not found");

            var branch = await _branchRepository.GetBranchByIdAsync(employee.BranchId);
            var bank = employee.BankId.HasValue ? await _employeeRepository.GetBankByIdAsync(employee.BankId.Value) : null;

            return Result<EmployeeDto>.Success(new EmployeeDto
            {
                Id = employee.Id,
                Name = employee.Name,
                Role = employee.Role,
                IsHaveNightShift = employee.IsHaveNightShift ?? false,
                theNameOfJob = employee.theNameOfJob,
                BankId = employee.BankId,
                BankName = bank?.Name,
                BankAccount = employee.BankAccount,
                ShiftHours = employee.ShiftHours,
                BranchId = employee.BranchId,
                BranchName = branch?.Name ?? string.Empty,
                EmployeeType = employee.EmployeeType ?? string.Empty,
                Status = employee.Status,
            });
        }

        public async Task<Result<PaginatedResponse<EmployeeDto>>> GetAllAsync(int page, int pageSize, int? branchId, int? bankId, string? role, string? name)
        {
            var employees = await _employeeRepository.GetAllAsync(page, pageSize, branchId, bankId, role, name);
            var totalCount = await _employeeRepository.GetTotalCountAsync(branchId, bankId, role, name);
            var dtos = new List<EmployeeDto>();
            foreach (var employee in employees)
            {
                var branch = await _branchRepository.GetBranchByIdAsync(employee.BranchId);
                var bank = employee.BankId.HasValue ? await _employeeRepository.GetBankByIdAsync(employee.BankId.Value) : null;
                dtos.Add(new EmployeeDto
                {
                    Id = employee.Id,
                    Name = employee.Name,
                    Role = employee.Role,
                    theNameOfJob = employee.theNameOfJob,
                    BankId = employee.BankId,
                    BankName = bank?.Name,
                    BankAccount = employee.BankAccount,
                    ShiftHours = employee.ShiftHours,
                    BranchId = employee.BranchId,
                    BranchName = branch?.Name ?? string.Empty,
                    EmployeeType = employee.EmployeeType ?? string.Empty

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

        public async Task<Result<bool>> UpdateAsync(int id, UpdateEmployeeDto dto, string userId, string userName)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee is null)
                return Result<bool>.Failure("Employee not found");

            // لازم ناخد القيم القديمة الأول قبل أي تعديل، عشان نقدر نعمل rollback يدوي لو حصلت مشكلة
            double? oldTarget = null;
            int? oldBranchId = null;

            if (dto.ShiftHours.HasValue || (dto.BranchId.HasValue && dto.BranchId.Value != employee.BranchId))
            {
                var currentMonthlyData = await _monthlyDataRepository.GetCurrentTargetAndBranchAsync(employee.Id);
                if (currentMonthlyData is null)
                    return Result<bool>.Failure($"لا يوجد سجل بيانات شهرية للموظف {employee.Id} لهذا الشهر");

                oldTarget = currentMonthlyData.Value.Target;
                oldBranchId = currentMonthlyData.Value.BranchId;
            }

            if (dto.IsHaveNightShift is not null) employee.IsHaveNightShift = dto.IsHaveNightShift;
            if (dto.Name is not null) employee.Name = dto.Name;
            if (dto.theNameOfJob is not null) employee.theNameOfJob = dto.theNameOfJob;
            if (dto.BankId.HasValue) employee.BankId = dto.BankId;
            if (dto.BankAccount is not null) employee.BankAccount = dto.BankAccount;

            // تحديث Target في MonthlyData لو الـ ShiftHours اتغيرت
            if (dto.ShiftHours.HasValue)
            {
                var newTarget = dto.ShiftHours.Value * 26;
                var targetResult = await _monthlyDataRepository.UpdateTargetAsync(employee.Id, newTarget);
                if (!targetResult.IsSuccess)
                    return Result<bool>.Failure(targetResult.Error!);

                employee.ShiftHours = dto.ShiftHours;
            }

            // تحديث BranchId في MonthlyData لو الفرع اتغير
            if (dto.BranchId.HasValue && dto.BranchId.Value != employee.BranchId)
            {
                var branchResult = await _monthlyDataRepository.UpdateBranchAsync(employee.Id, dto.BranchId.Value);
                if (!branchResult.IsSuccess)
                {
                    if (dto.ShiftHours.HasValue && oldTarget.HasValue)
                        await _monthlyDataRepository.UpdateTargetAsync(employee.Id, oldTarget.Value);

                    return Result<bool>.Failure(branchResult.Error!);
                }

                employee.BranchId = dto.BranchId.Value;
                await _employeeRepository.AddEmployeeBranchAsync(new EmployeeBranch
                {
                    EmployeeId = employee.Id,
                    BranchId = dto.BranchId.Value,
                    StartDate = DateTime.UtcNow
                });
            }

            if (dto.EmployeeType is not null)
            {
                employee.EmployeeType = dto.EmployeeType;
            }
            if (dto.Status is not null)
            {
                employee.Status = dto.Status;
            }

            await _auditService.LogDetailsAsync(userId, userName, $"تعديل بيانات الموظف {employee.Name}");

            // آخر خطوة: نحفظ الـ Employee. لو فشلت (false) أو حصل Exception، نرجع كل تعديلات MonthlyData للقيم القديمة
            try
            {
                var employeeUpdateSucceeded = await _employeeRepository.UpdateAsync(employee);
                if (!employeeUpdateSucceeded)
                {
                    await CompensateMonthlyDataAsync(employee.Id, dto, oldTarget, oldBranchId);
                    return Result<bool>.Failure("فشل تحديث بيانات الموظف");
                }
            }
            catch (Exception)
            {
                await CompensateMonthlyDataAsync(employee.Id, dto, oldTarget, oldBranchId);
                throw; // نرمي الاستثناء تاني عشان الـ middleware/logging العام يتعامل معاه زي أي exception تاني في السيستم
            }

            return Result<bool>.Success(true);
        }

        private async Task CompensateMonthlyDataAsync(int employeeId, UpdateEmployeeDto dto, double? oldTarget, int? oldBranchId)
        {
            if (dto.ShiftHours.HasValue && oldTarget.HasValue)
                await _monthlyDataRepository.UpdateTargetAsync(employeeId, oldTarget.Value);

            if (dto.BranchId.HasValue && oldBranchId.HasValue && dto.BranchId.Value != oldBranchId.Value)
                await _monthlyDataRepository.UpdateBranchAsync(employeeId, oldBranchId.Value);
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

            // تحديث Status الموظف لـ Stopped
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee is not null)
            {
                employee.Status = "Stopped";
                await _employeeRepository.UpdateAsync(employee);
            }
            return Result<bool>.Success(true);
        }

        public async Task<Result<IList<EmployeeBranchDto>>> GetEmployeeBranchesAsync(int employeeId)
        {
            var branches = await _employeeRepository.GetEmployeeBranchesAsync(employeeId);

            var dtos = new List<EmployeeBranchDto>();
            foreach (var branch in branches)
            {
                var branchInfo = await _branchRepository.GetBranchByIdAsync(branch.BranchId);
                dtos.Add(new EmployeeBranchDto
                {
                    Id = branch.Id,
                    EmployeeId = branch.EmployeeId,
                    BranchId = branch.BranchId,
                    BranchName = branchInfo?.Name ?? string.Empty,
                    StartDate = branch.StartDate
                });
            }

            return Result<IList<EmployeeBranchDto>>.Success(dtos);
        }

        public async Task<Result<bool>> ImportEmployeesData()
        {
            var json = await File.ReadAllTextAsync("C:\\Users\\ebrah\\source\\repos\\PharmacyHR\\Employees.Application\\EmployeesData\\create_employee_dto_import_final_clean.json");

            var employees = JsonSerializer.Deserialize<List<CreateEmployeeDto>>(json);

            foreach (var employee in employees!)
            {
                await CreateAsync(employee);
            }
            return Result<bool>.Success(true);
        }

        public Task<Result<bool>> DeleteAsync(int id)
        {

            var DeleteEmployeeTask = _employeeRepository.DeleteAsync(id);
            return DeleteEmployeeTask.ContinueWith(task =>
            {
                if (DeleteEmployeeTask.Result == true)
                {
                    var result = _authRepository.DeleteUser(id);
                    if (result.Result == true)
                    {
                        return Result<bool>.Success(true);
                    }
                    else
                    {
                        return Result<bool>.Failure("Failed to delete user");

                    }
                }
                else
                {
                    return Result<bool>.Failure("Failed to delete employee");
                }
            });
        }
    }
}
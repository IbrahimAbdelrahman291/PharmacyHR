using Requests.Application.Interfaces;
using Requests.Domain.Entities;
using Requests.Domain.Interfaces;
using SharedKernel.Interfaces;
using SharedKernel.Wrappers;

namespace Requests.Application.DTOs
{
    public class ForgetedHoursService : IForgetedHoursService
    {
        private readonly IForgetedHoursRepository _repository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMonthlyDataRepository _monthlyDataRepository;
        private readonly IBranchRepository _branchRepository;

        public ForgetedHoursService(
            IForgetedHoursRepository repository,
            IEmployeeRepository employeeRepository,
            IMonthlyDataRepository monthlyDataRepository,
            SharedKernel.Interfaces.IBranchRepository branchRepository
            )
        {
            _repository = repository;
            _employeeRepository = employeeRepository;
            _monthlyDataRepository = monthlyDataRepository;
            _branchRepository = branchRepository;
        }

        public async Task<Result<bool>> AddAsync(int employeeId, CreateForgetedHoursDto dto)
        {
            var validReasons = new[] { "انقطاع الانترنت", "انقطاع التيار الكهربي", "أخرى", "سهو البصمة" };
            if (!validReasons.Contains(dto.Reason))
                return Result<bool>.Failure("Invalid reason");

            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            var monthlyCount = await _repository.GetMonthlyCountAsync(employeeId, egyptNow.Month, egyptNow.Year);
            if (monthlyCount >= 3)
                return Result<bool>.Failure("لقد تجاوزت الحد المسموح به من الطلبات في هذا الشهر");

            var request = new ForgetedHoursRequest
            {
                EmployeeId = employeeId,
                Reason = dto.Reason,
                Notes = dto.Notes,
                ShiftDate = dto.ShiftDate,
                RequestDate = egyptNow,
                Status = "Pending",
                IsSeenByHR = false
            };

            await _repository.AddAsync(request);
            return Result<bool>.Success(true);
        }

        public async Task<Result<PaginatedResponse<ForgetedHoursDto>>> GetAllAsync(int? employeeId, bool? isSeenByHR, int page, int pageSize)
        {
            var requests = await _repository.GetAllAsync(employeeId, isSeenByHR, page, pageSize);
            var totalCount = await _repository.GetTotalCountAsync(employeeId, isSeenByHR);

            var dtos = new List<ForgetedHoursDto>();
            foreach (var request in requests)
            {
                var employeeInfo = await _employeeRepository.GetEmployeeBasicInfoAsync(request.EmployeeId);
                if (!employeeInfo.HasValue)
                {
                    continue;
                }
                var brachName = await _branchRepository.GetBranchByIdAsync(employeeInfo.Value.BranchId);
                dtos.Add(new ForgetedHoursDto
                {
                    Id = request.Id,
                    EmployeeId = request.EmployeeId,
                    EmployeeName = employeeInfo?.Name ?? string.Empty,
                    BranchName = brachName?.Name ?? string.Empty,
                    Reason = request.Reason,
                    Notes = request.Notes,
                    ShiftDate = request.ShiftDate,
                    RequestDate = request.RequestDate,
                    Status = request.Status,
                    RejectionReason = request.RejectionReason,
                    IsSeenByHR = request.IsSeenByHR
                });
            }

            return Result<PaginatedResponse<ForgetedHoursDto>>.Success(new PaginatedResponse<ForgetedHoursDto>
            {
                Data = dtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        public async Task<Result<bool>> ApproveOrRejectAsync(int id, ApproveRejectDto dto)
        {
            var request = await _repository.GetByIdAsync(id);
            if (request is null)
                return Result<bool>.Failure("Request not found");

            if (dto.IsApproved)
            {
                request.Status = "Approved";
                request.IsSeenByHR = true;
                request.IsSeenByEmployee = false;

                var shiftHours = await _employeeRepository.GetShiftHoursAsync(request.EmployeeId);
                await _monthlyDataRepository.AddForgetedHoursAsync(request.EmployeeId, shiftHours ?? 0);
            }
            else
            {
                request.Status = "Rejected";
                request.RejectionReason = dto.RejectionReason;
                request.IsSeenByHR = true;
                request.IsSeenByEmployee = false;
            }

            await _repository.UpdateAsync(request);
            return Result<bool>.Success(true);
        }

        public async Task<Result<int>> GetUnseenCountAsync(string role, int? employeeId)
        {
            var count = await _repository.GetUnseenCountAsync(role, employeeId);
            return Result<int>.Success(count);
        }

        public async Task<Result<bool>> MarkAsSeenAsync(int id, string role)
        {
            var request = await _repository.GetByIdAsync(id);
            if (request is null)
                return Result<bool>.Failure("Request not found");

            if (role == "HR")
            {
                request.IsSeenByHR = true;
            }
            else if (role == "Employee") 
            {
                request.IsSeenByEmployee = true;
            }

            await _repository.UpdateAsync(request);
            return Result<bool>.Success(true);
        }
    }
}

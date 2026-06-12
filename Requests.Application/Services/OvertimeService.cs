using Requests.Application.DTOs;
using Requests.Application.Interfaces;
using Requests.Domain.Entities;
using Requests.Domain.Interfaces;
using SharedKernel.Interfaces;
using SharedKernel.Wrappers;

namespace Requests.Application.Services
{
    public class OvertimeService : IOvertimeService
    {
        private readonly IOvertimeRepository _repository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMonthlyDataRepository _monthlyDataRepository;
        private readonly IAuthRepository _authRepository;

        public OvertimeService(
            IOvertimeRepository repository,
            IEmployeeRepository employeeRepository,
            IMonthlyDataRepository monthlyDataRepository,
            IAuthRepository authRepository)
        {
            _repository = repository;
            _employeeRepository = employeeRepository;
            _monthlyDataRepository = monthlyDataRepository;
            _authRepository = authRepository;
        }

        public async Task<Result<bool>> AddAsync(int employeeId, CreateOvertimeRequestDto dto)
        {
            var employeeInfo = await _employeeRepository.GetEmployeeBasicInfoAsync(employeeId);
            if (employeeInfo is null)
                return Result<bool>.Failure("Employee not found");

            var areaManagerUserId = await _authRepository.GetAreaManagerByBranchIdAsync(employeeInfo.Value.BranchId);

            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            var request = new OvertimeRequest
            {
                EmployeeId = employeeId,
                Hours = dto.Hours,
                Notes = dto.Notes,
                RequestDate = egyptNow,
                Status = "Pending",
                AreaManagerUserId = areaManagerUserId,
                IsSeenByHR = false,
                IsSeenByControl = false,
                IsSeenByAreaManager = false,
                IsSeenByEmployee = true
            };

            await _repository.AddAsync(request);
            return Result<bool>.Success(true);
        }

        public async Task<Result<PaginatedResponse<OvertimeRequestDto>>> GetAllAsync(int? employeeId, string? userId, string role, int page, int pageSize)
        {
            var requests = await _repository.GetAllAsync(employeeId, userId, role, page, pageSize);
            var totalCount = await _repository.GetTotalCountAsync(employeeId, userId, role);

            var dtos = new List<OvertimeRequestDto>();
            foreach (var request in requests)
            {
                var employeeInfo = await _employeeRepository.GetEmployeeBasicInfoAsync(request.EmployeeId);
                dtos.Add(new OvertimeRequestDto
                {
                    Id = request.Id,
                    EmployeeId = request.EmployeeId,
                    EmployeeName = employeeInfo?.Name ?? string.Empty,
                    Hours = request.Hours,
                    Notes = request.Notes,
                    RequestDate = request.RequestDate,
                    Status = request.Status,
                    ControlApproval = request.ControlApproval,
                    ControlRejectionReason = request.ControlRejectionReason,
                    AreaManagerApproval = request.AreaManagerApproval,
                    AreaManagerRejectionReason = request.AreaManagerRejectionReason,
                    HRApproval = request.HRApproval,
                    HRRejectionReason = request.HRRejectionReason,
                    IsSeenByHR = request.IsSeenByHR,
                    IsSeenByControl = request.IsSeenByControl,
                    IsSeenByAreaManager = request.IsSeenByAreaManager
                });
            }

            return Result<PaginatedResponse<OvertimeRequestDto>>.Success(new PaginatedResponse<OvertimeRequestDto>
            {
                Data = dtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        public async Task<Result<bool>> ControlApproveAsync(int id, string controlUserId, ApproveRejectDto dto)
        {
            var request = await _repository.GetByIdAsync(id);
            if (request is null)
                return Result<bool>.Failure("Request not found");

            request.ControlApproval = dto.IsApproved ? "Approved" : "Rejected";
            request.ControlUserId = controlUserId;
            request.ControlRejectionReason = dto.RejectionReason;
            request.IsSeenByControl = true;

            if (!dto.IsApproved)
                request.Status = "ControlRejected";
            else
                request.Status = "ControlApproved";

            await _repository.UpdateAsync(request);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> AreaManagerApproveAsync(int id, string areaManagerUserId, ApproveRejectDto dto)
        {
            var request = await _repository.GetByIdAsync(id);
            if (request is null)
                return Result<bool>.Failure("Request not found");

            if (request.AreaManagerUserId != areaManagerUserId)
                return Result<bool>.Failure("غير مسموح لك بالموافقة على هذا الطلب");

            request.AreaManagerApproval = dto.IsApproved ? "Approved" : "Rejected";
            request.AreaManagerRejectionReason = dto.RejectionReason;
            request.IsSeenByAreaManager = true;

            if (!dto.IsApproved)
                request.Status = "AreaManagerRejected";
            else
                request.Status = "AreaManagerApproved";

            await _repository.UpdateAsync(request);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> HRApproveAsync(int id, ApproveRejectDto dto)
        {
            var request = await _repository.GetByIdAsync(id);
            if (request is null)
                return Result<bool>.Failure("Request not found");

            request.HRApproval = dto.IsApproved ? "Approved" : "Rejected";
            request.HRRejectionReason = dto.RejectionReason;
            request.IsSeenByHR = true;
            request.IsSeenByEmployee = false;
            if (dto.IsApproved)
            {
                request.Status = "Approved";
                await _monthlyDataRepository.UpdateHoursOverTimeAsync(request.EmployeeId, request.Hours);
            }
            else
            {
                request.Status = "Rejected";
            }

            await _repository.UpdateAsync(request);
            return Result<bool>.Success(true);
        }

        public async Task<Result<int>> GetUnseenCountAsync(string? userId, string role)
        {
            var count = await _repository.GetUnseenCountAsync(userId, role);
            return Result<int>.Success(count);
        }

        public async Task<Result<bool>> MarkAsSeenAsync(int id, string role)
        {
            var request = await _repository.GetByIdAsync(id);
            if (request is null)
                return Result<bool>.Failure("Request not found");

            if (role == "HR") request.IsSeenByHR = true;
            else if (role == "Control") request.IsSeenByControl = true;
            else if (role == "AreaManager") request.IsSeenByAreaManager = true;
            else if (role == "Employee") request.IsSeenByEmployee = true;
            await _repository.UpdateAsync(request);
            return Result<bool>.Success(true);
        }
    }
}
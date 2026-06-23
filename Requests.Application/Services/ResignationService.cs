using Requests.Application.DTOs;
using Requests.Application.Interfaces;
using Requests.Domain.Entities;
using Requests.Domain.Interfaces;
using SharedKernel.Interfaces;
using SharedKernel.Wrappers;

namespace Requests.Application.Services
{
    public class ResignationService : IResignationService
    {
        private readonly IResignationRepository _repository;
        private readonly IEmployeeRepository _employeeRepository;

        public ResignationService(
            IResignationRepository repository,
            IEmployeeRepository employeeRepository)
        {
            _repository = repository;
            _employeeRepository = employeeRepository;
        }

        public async Task<Result<bool>> AddAsync(int employeeId, CreateResignationDto dto)
        {
            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            var request = new ResignationRequest
            {
                EmployeeId = employeeId,
                Reason = dto.Reason,
                RequestDate = egyptNow,
                Status = "Pending",
                IsSeenByHR = false,
                IsSeenByEmployee = true
            };

            await _repository.AddAsync(request);
            return Result<bool>.Success(true);
        }

        public async Task<Result<PaginatedResponse<ResignationDto>>> GetAllAsync(int? employeeId, bool? isSeenByHR, int page, int pageSize)
        {
            var requests = await _repository.GetAllAsync(employeeId, isSeenByHR, page, pageSize);
            var totalCount = await _repository.GetTotalCountAsync(employeeId, isSeenByHR);

            var dtos = new List<ResignationDto>();
            foreach (var request in requests)
            {
                var employeeInfo = await _employeeRepository.GetEmployeeBasicInfoAsync(request.EmployeeId);
                dtos.Add(new ResignationDto
                {
                    Id = request.Id,
                    EmployeeId = request.EmployeeId,
                    EmployeeName = employeeInfo?.Name ?? string.Empty,
                    Reason = request.Reason,
                    RequestDate = request.RequestDate,
                    Status = request.Status,
                    RejectionReason = request.RejectionReason,
                    IsSeenByHR = request.IsSeenByHR
                });
            }

            return Result<PaginatedResponse<ResignationDto>>.Success(new PaginatedResponse<ResignationDto>
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

            request.Status = dto.IsApproved ? "Approved" : "Rejected";
            request.RejectionReason = dto.RejectionReason;
            request.IsSeenByHR = true;
            request.IsSeenByEmployee = false;

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
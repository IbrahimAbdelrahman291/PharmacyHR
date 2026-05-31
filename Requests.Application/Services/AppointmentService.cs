using Requests.Application.DTOs;
using Requests.Application.Interfaces;
using Requests.Domain.Entities;
using Requests.Domain.Interfaces;
using SharedKernel.Interfaces;
using SharedKernel.Wrappers;

namespace Requests.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _repository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IEmployeeTypeRepository _employeeTypeRepository;

        public AppointmentService(
            IAppointmentRepository repository,
            IEmployeeRepository employeeRepository,
            IEmployeeTypeRepository employeeTypeRepository)
        {
            _repository = repository;
            _employeeRepository = employeeRepository;
            _employeeTypeRepository = employeeTypeRepository;
        }

        public async Task<Result<bool>> AddAsync(string areaManagerUserId, CreateAppointmentRequestDto dto)
        {
            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            var request = new AppointmentRequest
            {
                EmployeeId = dto.EmployeeId,
                AreaManagerUserId = areaManagerUserId,
                RequestDate = egyptNow,
                Status = "Pending",
                IsSeenByHR = false
            };

            await _repository.AddAsync(request);
            return Result<bool>.Success(true);
        }

        public async Task<Result<PaginatedResponse<AppointmentRequestDto>>> GetAllAsync(bool? isSeenByHR, int page, int pageSize)
        {
            var requests = await _repository.GetAllAsync(isSeenByHR, page, pageSize);
            var totalCount = await _repository.GetTotalCountAsync(isSeenByHR);

            var dtos = new List<AppointmentRequestDto>();
            foreach (var request in requests)
            {
                var employeeInfo = await _employeeRepository.GetEmployeeBasicInfoAsync(request.EmployeeId);
                dtos.Add(new AppointmentRequestDto
                {
                    Id = request.Id,
                    EmployeeId = request.EmployeeId,
                    EmployeeName = employeeInfo?.Name ?? string.Empty,
                    AreaManagerUserId = request.AreaManagerUserId,
                    RequestDate = request.RequestDate,
                    Status = request.Status,
                    RejectionReason = request.RejectionReason,
                    IsSeenByHR = request.IsSeenByHR
                });
            }

            return Result<PaginatedResponse<AppointmentRequestDto>>.Success(new PaginatedResponse<AppointmentRequestDto>
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
                await _employeeTypeRepository.UpdateEmployeeTypeAsync(request.EmployeeId, "تم التعيين");
            }
            else
            {
                request.Status = "Rejected";
                request.RejectionReason = dto.RejectionReason;
                request.IsSeenByHR = true;
            }

            await _repository.UpdateAsync(request);
            return Result<bool>.Success(true);
        }

        public async Task<Result<int>> GetUnseenCountAsync()
        {
            var count = await _repository.GetUnseenCountAsync();
            return Result<int>.Success(count);
        }

        public async Task<Result<bool>> MarkAsSeenAsync(int id)
        {
            var request = await _repository.GetByIdAsync(id);
            if (request is null)
                return Result<bool>.Failure("Request not found");

            request.IsSeenByHR = true;
            await _repository.UpdateAsync(request);
            return Result<bool>.Success(true);
        }
    }
}
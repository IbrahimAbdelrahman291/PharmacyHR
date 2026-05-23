using Requests.Application.DTOs;
using Requests.Application.Interfaces;
using Requests.Domain.Entities;
using Requests.Domain.Interfaces;
using SharedKernel.Interfaces;
using SharedKernel.Wrappers;

namespace Requests.Application.Services
{
    public class ComplaintService : IComplaintService
    {
        private readonly IComplaintRepository _repository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IAuthRepository _authRepository;

        public ComplaintService(
            IComplaintRepository repository,
            IEmployeeRepository employeeRepository,
            IAuthRepository authRepository)
        {
            _repository = repository;
            _employeeRepository = employeeRepository;
            _authRepository = authRepository;
        }

        public async Task<Result<bool>> AddAsync(int employeeId, CreateComplaintDto dto)
        {
            var validRecipients = new[] { "HR", "AreaManager", "CEO" };
            if (!validRecipients.Contains(dto.RecipientRole))
                return Result<bool>.Failure("Invalid recipient");

            string? recipientUserId = null;
            if (dto.RecipientRole == "AreaManager")
            {
                var employeeInfo = await _employeeRepository.GetEmployeeBasicInfoAsync(employeeId);
                recipientUserId = await _authRepository.GetAreaManagerByBranchIdAsync(employeeInfo!.Value.BranchId);
            }

            var complaint = new ComplaintRequest
            {
                EmployeeId = employeeId,
                Content = dto.Content,
                RecipientRole = dto.RecipientRole,
                RecipientUserId = recipientUserId,
                Status = "Pending",
                Date = DateTime.UtcNow,
                IsSeenByHR = false
            };

            await _repository.AddAsync(complaint);
            return Result<bool>.Success(true);
        }

        public async Task<Result<PaginatedResponse<ComplaintDto>>> GetAllAsync(int? employeeId, bool? isSeenByHR, int page, int pageSize)
        {
            var complaints = await _repository.GetAllAsync(employeeId, isSeenByHR, page, pageSize);
            var totalCount = await _repository.GetTotalCountAsync(employeeId, isSeenByHR);

            var dtos = new List<ComplaintDto>();
            foreach (var complaint in complaints)
            {
                var employeeInfo = await _employeeRepository.GetEmployeeBasicInfoAsync(complaint.EmployeeId);
                dtos.Add(new ComplaintDto
                {
                    Id = complaint.Id,
                    EmployeeId = complaint.EmployeeId,
                    EmployeeName = employeeInfo?.Name ?? string.Empty,
                    Content = complaint.Content,
                    RecipientRole = complaint.RecipientRole,
                    Status = complaint.Status,
                    Response = complaint.Response,
                    Date = complaint.Date,
                    IsSeenByHR = complaint.IsSeenByHR
                });
            }

            return Result<PaginatedResponse<ComplaintDto>>.Success(new PaginatedResponse<ComplaintDto>
            {
                Data = dtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        public async Task<Result<bool>> RespondAsync(int id, RespondComplaintDto dto)
        {
            var complaint = await _repository.GetByIdAsync(id);
            if (complaint is null)
                return Result<bool>.Failure("Complaint not found");

            complaint.Response = dto.Response;
            complaint.Status = dto.Status;
            complaint.IsSeenByHR = true;

            await _repository.UpdateAsync(complaint);
            return Result<bool>.Success(true);
        }

        public async Task<Result<int>> GetUnseenCountAsync()
        {
            var count = await _repository.GetUnseenCountAsync();
            return Result<int>.Success(count);
        }

        public async Task<Result<bool>> MarkAsSeenAsync(int id)
        {
            var complaint = await _repository.GetByIdAsync(id);
            if (complaint is null)
                return Result<bool>.Failure("Complaint not found");

            complaint.IsSeenByHR = true;
            await _repository.UpdateAsync(complaint);
            return Result<bool>.Success(true);
        }
    }
}
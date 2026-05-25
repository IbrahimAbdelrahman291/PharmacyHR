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

        public async Task<Result<PaginatedResponse<ComplaintDto>>> GetAllAsync(int? employeeId, bool? isSeenByHR, string? recipientUserId, string role, int page, int pageSize)
        {
            string? filterUserId = null;
            string? filterRole = null;

            if (role == "AreaManager")
                filterUserId = recipientUserId;
            else if (role == "CEO")
                filterRole = "CEO";

            var complaints = await _repository.GetAllAsync(employeeId, isSeenByHR, filterUserId, filterRole, page, pageSize);
            var totalCount = await _repository.GetTotalCountAsync(employeeId, isSeenByHR, filterUserId);

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

        public async Task<Result<bool>> RespondAsync(int id, RespondComplaintDto dto, string userId, string role)
        {
            var complaint = await _repository.GetByIdAsync(id);
            if (complaint is null)
                return Result<bool>.Failure("Complaint not found");

            if (role == "AreaManager" || role == "CEO")
            {
                if (complaint.RecipientUserId != userId)
                    return Result<bool>.Failure("غير مسموح لك بالرد على هذه الشكوى");
            }

            complaint.Response = dto.Response;
            complaint.Status = dto.Status;

            if (role == "HR") complaint.IsSeenByHR = true;
            else if (role == "AreaManager") complaint.IsSeenByAreaManager = true;
            else if (role == "CEO") complaint.IsSeenByCEO = true;

            await _repository.UpdateAsync(complaint);
            return Result<bool>.Success(true);
        }

        public async Task<Result<int>> GetUnseenCountAsync(string? recipientUserId, string role)
        {
            var count = await _repository.GetUnseenCountAsync(recipientUserId, role);
            return Result<int>.Success(count);
        }

        public async Task<Result<bool>> MarkAsSeenAsync(int id, string userId, string role)
        {
            var complaint = await _repository.GetByIdAsync(id);
            if (complaint is null)
                return Result<bool>.Failure("Complaint not found");

            if (role == "HR") complaint.IsSeenByHR = true;
            else if (role == "AreaManager")
            {
                if (complaint.RecipientUserId != userId)
                    return Result<bool>.Failure("غير مسموح لك");
                complaint.IsSeenByAreaManager = true;
            }
            else if (role == "CEO")
            {
                if (complaint.RecipientUserId != userId)
                    return Result<bool>.Failure("غير مسموح لك");
                complaint.IsSeenByCEO = true;
            }

            await _repository.UpdateAsync(complaint);
            return Result<bool>.Success(true);
        }
    }
}
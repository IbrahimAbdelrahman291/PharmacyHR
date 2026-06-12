using Requests.Application.DTOs;
using Requests.Application.Interfaces;
using Requests.Domain.Entities;
using Requests.Domain.Interfaces;
using SharedKernel.Interfaces;
using SharedKernel.Wrappers;

namespace Requests.Application.Services
{
    public class BorrowService : IBorrowService
    {
        private readonly IBorrowRepository _repository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMonthlyDataRepository _monthlyDataRepository;

        public BorrowService(
            IBorrowRepository repository,
            IEmployeeRepository employeeRepository,
            IMonthlyDataRepository monthlyDataRepository)
        {
            _repository = repository;
            _employeeRepository = employeeRepository;
            _monthlyDataRepository = monthlyDataRepository;
        }

        public async Task<Result<bool>> AddBorrowRequestAsync(int employeeId, CreateBorrowRequestDto dto)
        {
            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            //تأكد إن الطلب في الفترة المسموح بيها(10 لـ 25)
            if (egyptNow.Day < 10 || egyptNow.Day > 25)
                return Result<bool>.Failure("يمكن تقديم طلب السلفة من يوم 10 إلى يوم 25 فقط");

            // تحقق لو المبلغ أكتر من ربع المرتب
            var QuarterSalary = await _monthlyDataRepository.GetTotalSalaryForInstallmentBorrow(employeeId);
            bool isOverQuarter = false;

            if (QuarterSalary is not null)
            {
                if (dto.Amount > QuarterSalary)
                    isOverQuarter = true;
            }

            var request = new BorrowRequest
            {
                EmployeeId = employeeId,
                Amount = dto.Amount,
                Notes = dto.Notes,
                RequestDate = egyptNow,
                Status = "Pending",
                IsSeenByHR = false,
                IsSeenByEmployee = true,
                IsOverQuarter = isOverQuarter
            };

            await _repository.AddBorrowRequestAsync(request);
            return Result<bool>.Success(true);
        }

        public async Task<Result<PaginatedResponse<BorrowRequestDto>>> GetAllBorrowRequestsAsync(int? employeeId, bool? isSeenByHR, int page, int pageSize)
        {
            var requests = await _repository.GetAllBorrowRequestsAsync(employeeId, isSeenByHR, page, pageSize);
            var totalCount = await _repository.GetTotalBorrowRequestsCountAsync(employeeId, isSeenByHR);

            var dtos = new List<BorrowRequestDto>();
            foreach (var request in requests)
            {
                var employeeInfo = await _employeeRepository.GetEmployeeBasicInfoAsync(request.EmployeeId);
                dtos.Add(new BorrowRequestDto
                {
                    Id = request.Id,
                    EmployeeId = request.EmployeeId,
                    EmployeeName = employeeInfo?.Name ?? string.Empty,
                    Amount = request.Amount,
                    Notes = request.Notes,
                    RequestDate = request.RequestDate,
                    Status = request.Status,
                    RejectionReason = request.RejectionReason,
                    IsSeenByHR = request.IsSeenByHR,
                    IsSeenByEmployee = request.IsSeenByEmployee,
                    IsOverQuarter = request.IsOverQuarter
                });
            }

            return Result<PaginatedResponse<BorrowRequestDto>>.Success(new PaginatedResponse<BorrowRequestDto>
            {
                Data = dtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        public async Task<Result<bool>> ApproveBorrowRequestAsync(int id, ApproveRejectDto dto)
        {
            var request = await _repository.GetBorrowRequestByIdAsync(id);
            if (request is null)
                return Result<bool>.Failure("Request not found");

            if (dto.IsApproved)
            {
                request.Status = "Approved";
                request.IsSeenByHR = true;
                request.IsSeenByEmployee = false; 
                await _monthlyDataRepository.AddBorrowAsync(request.EmployeeId, request.Amount);
            }
            else
            {
                request.Status = "Rejected";
                request.RejectionReason = dto.RejectionReason;
                request.IsSeenByHR = true;
                request.IsSeenByEmployee = false; 
            }

            await _repository.UpdateBorrowRequestAsync(request);
            return Result<bool>.Success(true);
        }

        public async Task<Result<int>> GetUnseenBorrowCountAsync(string role)
        {
            var count = await _repository.GetUnseenBorrowCountAsync(role);
            return Result<int>.Success(count);
        }

        public async Task<Result<bool>> MarkBorrowAsSeenAsync(int id, string role)
        {
            var request = await _repository.GetBorrowRequestByIdAsync(id);
            if (request is null)
                return Result<bool>.Failure("Request not found");

            if (role == "HR")
            {
                request.IsSeenByHR = true;
            }
            else if(role == "Employee")
            {
                request.IsSeenByEmployee = true;
            }
            await _repository.UpdateBorrowRequestAsync(request);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> AddInstallmentBorrowAsync(CreateInstallmentBorrowDto dto)
        {
            // تحقق لو الموظف عنده استقالة مقبولة
            var monthlyAmount = dto.TotalAmount / dto.TotalMonths;

            var borrow = new InstallmentBorrow
            {
                EmployeeId = dto.EmployeeId,
                TotalAmount = dto.TotalAmount,
                MonthlyAmount = monthlyAmount,
                TotalMonths = dto.TotalMonths,
                RemainingMonths = dto.TotalMonths,
                StartMonth = dto.StartMonth,
                StartYear = dto.StartYear,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                HasResignation = false // هنتحقق منها لاحقاً
            };

            await _repository.AddInstallmentBorrowAsync(borrow);
            return Result<bool>.Success(true);
        }

        public async Task<Result<IList<InstallmentBorrowDto>>> GetInstallmentBorrowsByEmployeeAsync(int employeeId)
        {
            var borrows = await _repository.GetInstallmentBorrowsByEmployeeAsync(employeeId);

            var dtos = new List<InstallmentBorrowDto>();
            foreach (var borrow in borrows)
            {
                var employeeInfo = await _employeeRepository.GetEmployeeBasicInfoAsync(borrow.EmployeeId);
                dtos.Add(new InstallmentBorrowDto
                {
                    Id = borrow.Id,
                    EmployeeId = borrow.EmployeeId,
                    EmployeeName = employeeInfo?.Name ?? string.Empty,
                    TotalAmount = borrow.TotalAmount,
                    MonthlyAmount = borrow.MonthlyAmount,
                    TotalMonths = borrow.TotalMonths,
                    RemainingMonths = borrow.RemainingMonths,
                    StartMonth = borrow.StartMonth,
                    StartYear = borrow.StartYear,
                    IsActive = borrow.IsActive,
                    HasResignation = borrow.HasResignation
                });
            }

            return Result<IList<InstallmentBorrowDto>>.Success(dtos);
        }

        public async Task ProcessMonthlyInstallmentsAsync()
        {
            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            var activeBorrows = await _repository.GetActiveInstallmentBorrowsAsync();

            foreach (var borrow in activeBorrows)
            {
                // تحقق إن الشهر الحالي >= شهر البداية
                var startDate = new DateTime(borrow.StartYear, borrow.StartMonth, 1);
                if (egyptNow < startDate) continue;

                await _monthlyDataRepository.AddBorrowAsync(borrow.EmployeeId, borrow.MonthlyAmount);

                borrow.RemainingMonths--;
                if (borrow.RemainingMonths <= 0)
                    borrow.IsActive = false;

                await _repository.UpdateInstallmentBorrowAsync(borrow);
            }
        }
    }
}
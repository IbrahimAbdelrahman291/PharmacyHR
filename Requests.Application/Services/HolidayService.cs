using Requests.Application.DTOs;
using Requests.Application.Interfaces;
using Requests.Domain.Entities;
using Requests.Domain.Interfaces;
using SharedKernel.Interfaces;
using SharedKernel.Wrappers;


namespace Requests.Application.Services
{
    public class HolidayService : IHolidayService
    {
        private readonly IHolidayRepository _repository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMonthlyDataRepository _monthlyDataRepository;
        private readonly IAuthRepository _authRepository;

        public HolidayService(
            IHolidayRepository repository,
            IEmployeeRepository employeeRepository,
            IMonthlyDataRepository monthlyDataRepository,
            IAuthRepository authRepository)
        {
            _repository = repository;
            _employeeRepository = employeeRepository;
            _monthlyDataRepository = monthlyDataRepository;
            _authRepository = authRepository;
        }

        public async Task<Result<bool>> AddAsync(int employeeId, CreateHolidayDto dto)
        {
            if (dto.ToDate < dto.FromDate)
                return Result<bool>.Failure("تاريخ النهاية يجب أن يكون بعد تاريخ البداية");

            var totalDays = (dto.ToDate.DayNumber - dto.FromDate.DayNumber) + 1;

            // تأكد إن الرصيد كافي
            var Holiday = await _monthlyDataRepository.GetHolidaysInCurrentMonthAsync(employeeId);
            if (Holiday is null)
                return Result<bool>.Failure("لا توجد بيانات شهرية للموظف");

            if ((Holiday ?? 0) < totalDays)
                return Result<bool>.Failure("رصيد الإجازات غير كافي");

            // جيب الـ Area Manager المسؤل عن الفرع
            var employeeInfo = await _employeeRepository.GetEmployeeBasicInfoAsync(employeeId);
            var areaManagerUserId = await _authRepository.GetAreaManagerByBranchIdAsync(employeeInfo!.Value.BranchId);

            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            var request = new HolidayRequest
            {
                EmployeeId = employeeId,
                FromDate = dto.FromDate,
                ToDate = dto.ToDate,
                TotalDays = totalDays,
                Status = "Pending",
                AreaManagerUserId = areaManagerUserId,
                RequestDate = egyptNow,
                IsSeenByHR = false,
                IsSeenByEmployee = true
            };

            await _repository.AddAsync(request);
            return Result<bool>.Success(true);
        }

        public async Task<Result<PaginatedResponse<HolidayDto>>> GetAllAsync(int? employeeId, bool? isSeenByHR, string? areaManagerUserId, string role, int page, int pageSize)
        {
            string? filterAreaManagerUserId = null;

            if (role == "AreaManager")
                filterAreaManagerUserId = areaManagerUserId;

            var requests = await _repository.GetAllAsync(employeeId, isSeenByHR, filterAreaManagerUserId, page, pageSize);
            var totalCount = await _repository.GetTotalCountAsync(employeeId, isSeenByHR, filterAreaManagerUserId);

            var dtos = new List<HolidayDto>();
            foreach (var request in requests)
            {
                var employeeInfo = await _employeeRepository.GetEmployeeBasicInfoAsync(request.EmployeeId);
                dtos.Add(new HolidayDto
                {
                    Id = request.Id,
                    EmployeeId = request.EmployeeId,
                    EmployeeName = employeeInfo?.Name ?? string.Empty,
                    FromDate = request.FromDate,
                    ToDate = request.ToDate,
                    TotalDays = request.TotalDays,
                    Status = request.Status,
                    AreaManagerApproval = request.AreaManagerApproval,
                    AreaManagerCover = request.AreaManagerCover,
                    RejectionReason = request.RejectionReason,
                    RequestDate = request.RequestDate,
                    IsSeenByHR = request.IsSeenByHR
                });
            }

            return Result<PaginatedResponse<HolidayDto>>.Success(new PaginatedResponse<HolidayDto>
            {
                Data = dtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        public async Task<Result<bool>> AreaManagerApproveAsync(int id, string areaManagerUserId, AreaManagerApproveHolidayDto dto)
        {
            var request = await _repository.GetByIdAsync(id);
            if (request is null)
                return Result<bool>.Failure("Request not found");

            if (request.AreaManagerUserId != areaManagerUserId)
                return Result<bool>.Failure("غير مسموح لك بالموافقة على هذا الطلب");

            if (dto.IsApproved)
            {
                if (string.IsNullOrEmpty(dto.Cover))
                    return Result<bool>.Failure("يجب تحديد من سيغطي مكان الموظف");

                request.AreaManagerApproval = "Approved";
                request.AreaManagerCover = dto.Cover;
                request.Status = "AreaManagerApproved";
            }
            else
            {
                request.AreaManagerApproval = "Rejected";
                request.RejectionReason = dto.RejectionReason;
                request.Status = "Rejected";
            }

            request.IsSeenByHR = false;
            await _repository.UpdateAsync(request);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> HRApproveAsync(int id, HRApproveHolidayDto dto)
        {
            var request = await _repository.GetByIdAsync(id);
            if (request is null)
                return Result<bool>.Failure("Request not found");

            if (dto.IsApproved)
            {
                request.Status = "Approved";
                request.IsSeenByHR = true;
                request.IsSeenByEmployee = false;
                // ضيف ساعات الإجازة
                var shiftHours = await _employeeRepository.GetShiftHoursAsync(request.EmployeeId);
                var totalHolidayHours = (shiftHours ?? 0) * request.TotalDays;
                await _monthlyDataRepository.AddHolidayHoursAsync(request.EmployeeId, totalHolidayHours, request.TotalDays);
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

            var count = await _repository.GetUnseenCountAsync(role,employeeId);
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

using Attendance.Application.DTOs;
using Attendance.Application.Interfaces;
using Attendance.Domain.Entities;
using Attendance.Domain.Interfaces;
using SharedKernel.Interfaces;
using SharedKernel.Wrappers;

namespace Attendance.Application.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IWorkLogRepository _workLogRepository;
        private readonly SharedKernel.Interfaces.IMonthlyDataRepository _monthlyDataRepository;
        private readonly SharedKernel.Interfaces.IEmployeeScheduleRepository _scheduleRepository;
        private readonly SharedKernel.Interfaces.IEmployeeRepository _employeeRepository;
        private readonly SharedKernel.Interfaces.IBranchRepository _branchRepository;

        public AttendanceService(
            IWorkLogRepository workLogRepository,
            IMonthlyDataRepository monthlyDataRepository,
            IEmployeeScheduleRepository scheduleRepository,
            SharedKernel.Interfaces.IEmployeeRepository employeeRepository,
            SharedKernel.Interfaces.IBranchRepository branchRepository)
        {
            _workLogRepository = workLogRepository;
            _monthlyDataRepository = monthlyDataRepository;
            _scheduleRepository = scheduleRepository;
            _employeeRepository = employeeRepository;
            _branchRepository = branchRepository;
        }

        public async Task<Result<bool>> StartShiftAsync(int employeeId)
        {
            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);
            var egyptDate = DateOnly.FromDateTime(egyptNow);
            var egyptTime = TimeOnly.FromDateTime(egyptNow);
            
            var schedule = await _scheduleRepository.GetEmployeeScheduleByDayAsync(employeeId, egyptDate.DayOfWeek);
            if (schedule is null)
                return Result<bool>.Failure("مش مسموح لك تسجل حضور النهارده");

            var allowedCheckIn = egyptDate.ToDateTime(schedule.Value.CheckInTime.AddMinutes(-15));


            var allowedCheckOut = egyptDate.ToDateTime(schedule.Value.CheckOutTime.AddMinutes(15));
            if (schedule.Value.CheckOutTime < schedule.Value.CheckInTime)
            {
                allowedCheckOut = allowedCheckOut.AddDays(1);
            }
            if (egyptNow < allowedCheckIn)
                return Result<bool>.Failure($"لسه مش متاح تسجل دلوقتي، موعد الحضور {schedule.Value.CheckInTime}");

            var openShift = await _workLogRepository.GetOpenShiftAsync(employeeId);
            if (openShift is not null)
                return Result<bool>.Failure("يوجد شيفت مفتوح بالفعل");

            var hasShiftToday = await _workLogRepository.HasShiftOnDayAsync(employeeId, egyptDate);
            if (hasShiftToday)
                return Result<bool>.Failure("تم تسجيل بداية العمل بالفعل لهذا اليوم");
            if (egyptNow > allowedCheckOut)
            {
                return Result<bool>.Failure("انتهى وقت تسجيل الحضور لهذا الشيفت");
            }

            var workLog = new WorkLog
            {
                EmployeeId = employeeId,
                Day = egyptDate,
                Start = egyptTime,
                IsEnd = false,
                End = TimeOnly.MinValue
            };

            await _workLogRepository.AddAsync(workLog);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> EndShiftAsync(int employeeId)
        {
            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);
            var egyptDate = DateOnly.FromDateTime(egyptNow);
            var egyptTime = TimeOnly.FromDateTime(egyptNow);
            var workLog = await _workLogRepository.GetOpenShiftAsync(employeeId);
            
            if (workLog is null)
                return Result<bool>.Failure("لا يوجد تسجيل حضور في هذا اليوم أو اليوم الذي يسبقه");

            var schedule = await _scheduleRepository.GetEmployeeScheduleByDayAsync(employeeId, workLog.Day.DayOfWeek);
            var CheckOutTime = schedule.Value.CheckOutTime;
            var startDateTime = workLog.Day.ToDateTime(workLog.Start);
            var endDateTime = egyptDate.ToDateTime(CheckOutTime);
            var allowedCheckOut = workLog.Day.ToDateTime(schedule.Value.CheckOutTime.AddMinutes(15));
            if (allowedCheckOut < workLog.Day.ToDateTime(schedule.Value.CheckInTime))
            {
                allowedCheckOut = allowedCheckOut.AddDays(1);
            }
            var totalWorkTime = allowedCheckOut - startDateTime;
            var endShift = egyptDate.ToDateTime(egyptTime);
            var totalTime = endShift - startDateTime;
            var allowCheckOutEmergency = startDateTime.AddMinutes(15);
           

            if (egyptNow >= allowCheckOutEmergency)
            {
                if (schedule is not null)
                {
                    if (egyptNow > allowedCheckOut)
                    {
                        workLog.End = CheckOutTime;
                        workLog.TotalTime = allowedCheckOut - startDateTime;
                        workLog.IsEnd = true;
                        await _workLogRepository.UpdateAsync(workLog);
                        await _monthlyDataRepository.AddHoursAsync(employeeId, totalWorkTime.TotalHours);

                        return Result<bool>.Success(true);
                    }
                }

                workLog.End = egyptTime;
                workLog.TotalTime = totalTime;
                workLog.IsEnd= true;
                await _workLogRepository.UpdateAsync(workLog);
                await _monthlyDataRepository.AddHoursAsync(employeeId, totalTime.TotalHours);

                return Result<bool>.Success(true);
            }
            else 
            {
                return Result<bool>.Failure("لا يمكنك انهاء الشيفت الان");
            }
        }

        public async Task<Result<PaginatedResponse<AttendanceReportDto>>> GetReportAsync(string type, DateOnly fromDate, DateOnly toDate, int? employeeId, int? branchId, int page, int pageSize)
        {
            var workLogs = await _workLogRepository.GetReportAsync(fromDate, toDate, employeeId);
            var totalCount = await _workLogRepository.GetReportCountAsync(fromDate, toDate, employeeId);

            var result = new List<AttendanceReportDto>();
            foreach (var workLog in workLogs)
            {
                var employeeInfo = await _employeeRepository.GetEmployeeBasicInfoAsync(workLog.EmployeeId);
                if (employeeInfo is null) continue;

                if (branchId.HasValue && employeeInfo.Value.BranchId != branchId.Value) continue;

                var branchInfo = await _branchRepository.GetBranchByIdAsync(employeeInfo.Value.BranchId);
                var schedule = await _scheduleRepository.GetEmployeeScheduleByDayAsync(workLog.EmployeeId, workLog.Day.DayOfWeek);

                var scheduledCheckIn = schedule?.CheckInTime ?? TimeOnly.MinValue;
                var scheduledCheckOut = schedule?.CheckOutTime ?? TimeOnly.MinValue;
                var actualCheckIn = workLog.Start == TimeOnly.MinValue ? (TimeOnly?)null : workLog.Start;
                
                var actualCheckOut = workLog.End == TimeOnly.MinValue ? (TimeOnly?)null : workLog.End;

                // فلتر حسب النوع
                if (type == "open" && actualCheckOut is not null) continue;
                if (type == "late" && (actualCheckIn is null || actualCheckIn <= scheduledCheckIn.AddMinutes(15))) continue;
                if (type == "overtime" && (actualCheckOut is null || actualCheckOut <= scheduledCheckOut)) continue;

                result.Add(new AttendanceReportDto
                {
                    EmployeeId = workLog.EmployeeId,
                    EmployeeName = employeeInfo.Value.Name,
                    BranchName = branchInfo?.Name ?? string.Empty,
                    Day = workLog.Day,
                    ScheduledCheckIn = scheduledCheckIn,
                    ActualCheckIn = actualCheckIn,
                    ScheduledCheckOut = scheduledCheckOut,
                    ActualCheckOut = actualCheckOut
                });
            }

            var paged = result.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Result<PaginatedResponse<AttendanceReportDto>>.Success(new PaginatedResponse<AttendanceReportDto>
            {
                Data = paged,
                TotalCount = result.Count,
                Page = page,
                PageSize = pageSize
            });
        }

        public async Task<Result<PaginatedResponse<AbsentReportDto>>> GetAbsentReportAsync(DateOnly fromDate, DateOnly toDate, int? branchId, int page, int pageSize, int? employeeId)
        {
            var result = new List<AbsentReportDto>();
            var currentDate = fromDate;

            while (currentDate <= toDate)
            {
                var workLogs = await _workLogRepository.GetReportAsync(currentDate, currentDate, null);
                var presentEmployeeIds = workLogs.Select(w => w.EmployeeId).ToHashSet();

                var schedules = await _scheduleRepository.GetAllEmployeesWithScheduleByDayAsync(currentDate.DayOfWeek,employeeId);

                foreach (var schedule in schedules)
                {
                    if (presentEmployeeIds.Contains(schedule.EmployeeId)) continue;

                    var employeeInfo = await _employeeRepository.GetEmployeeBasicInfoAsync(schedule.EmployeeId);
                    if (employeeInfo is null) continue;
                    if (branchId.HasValue && employeeInfo.Value.BranchId != branchId.Value) continue;

                    var branchInfo = await _branchRepository.GetBranchByIdAsync(employeeInfo.Value.BranchId);

                    result.Add(new AbsentReportDto
                    {
                        EmployeeId = schedule.EmployeeId,
                        EmployeeName = employeeInfo.Value.Name,
                        BranchName = branchInfo?.Name ?? string.Empty,
                        Day = currentDate,
                        ScheduledCheckIn = schedule.CheckInTime,
                        ScheduledCheckOut = schedule.CheckOutTime
                    });
                }

                currentDate = currentDate.AddDays(1);
            }

            var paged = result.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Result<PaginatedResponse<AbsentReportDto>>.Success(new PaginatedResponse<AbsentReportDto>
            {
                Data = paged,
                TotalCount = result.Count,
                Page = page,
                PageSize = pageSize
            });
        }

        public async Task<Result<PaginatedResponse<WorkLogDto>>> GetMyShiftsAsync(int employeeId, DateOnly fromDate, DateOnly toDate, int page, int pageSize)
        {
            var workLogs = await _workLogRepository.GetReportAsync(fromDate, toDate, employeeId);
            var totalCount = await _workLogRepository.GetReportCountAsync(fromDate, toDate, employeeId);

            var dtos = workLogs.Select(w => new WorkLogDto
            {
                Id = w.Id,
                EmployeeId = w.EmployeeId,
                Day = w.Day,
                Start = w.Start,
                End = w.End,
                TotalHours = w.TotalTime.TotalHours
            }).Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Result<PaginatedResponse<WorkLogDto>>.Success(new PaginatedResponse<WorkLogDto>
            {
                Data = dtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }
    }
}
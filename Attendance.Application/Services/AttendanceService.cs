using Attendance.Application.DTOs;
using Attendance.Application.Interfaces;
using Attendance.Domain.Entities;
using Attendance.Domain.Interfaces;
using SharedKernel.Interfaces;
using SharedKernel.Wrappers;
using System;
using System.Collections.Generic;
using System.Text;

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

            // تأكد إن اليوم موجود في الـ Schedule
            var schedule = await _scheduleRepository.GetEmployeeScheduleByDayAsync(employeeId, egyptDate.DayOfWeek);
            if (schedule is null)
                return Result<bool>.Failure("مش مسموح لك تسجل حضور النهارده");

            // تأكد إن الوقت الحالي >= CheckInTime - 15 دقيقة
            var allowedCheckIn = schedule.Value.CheckInTime.AddMinutes(-15);
            if (egyptTime < allowedCheckIn)
                return Result<bool>.Failure($"لسه مش متاح تسجل دلوقتي، موعد الحضور {schedule.Value.CheckInTime}");

            // تأكد مفيش شيفت مفتوح
            var openShift = await _workLogRepository.GetOpenShiftAsync(employeeId);
            if (openShift is not null)
                return Result<bool>.Failure("يوجد شيفت مفتوح بالفعل");

            // تأكد مفيش شيفت مسجل النهارده
            var hasShiftToday = await _workLogRepository.HasShiftOnDayAsync(employeeId, egyptDate);
            if (hasShiftToday)
                return Result<bool>.Failure("تم تسجيل بداية العمل بالفعل لهذا اليوم");

            var workLog = new WorkLog
            {
                EmployeeId = employeeId,
                Day = egyptDate,
                Start = egyptTime,
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

            // بيدور على شيفت مفتوح النهارده أو امبارح
            var workLog = await _workLogRepository.GetOpenShiftAsync(employeeId);
            if (workLog is null)
                return Result<bool>.Failure("لا يوجد تسجيل حضور في هذا اليوم أو اليوم الذي يسبقه");

            // جيب الـ Schedule بتاع يوم الشيفت
            var schedule = await _scheduleRepository.GetEmployeeScheduleByDayAsync(employeeId, egyptDate.DayOfWeek);
            if (schedule is not null)
            {
                var allowedCheckOut = schedule.Value.CheckOutTime.AddMinutes(15);
                if (egyptTime > allowedCheckOut)
                    return Result<bool>.Failure("اتواصل مع HR عشان تسجل انصرافك");
            }

            // حساب TotalTime
            var startDateTime = workLog.Day.ToDateTime(workLog.Start);
            var totalWorkTime = egyptNow - startDateTime;

            // لو أكتر من 24 ساعة → رفض
            if (totalWorkTime.TotalHours >= 24)
                return Result<bool>.Failure("تعذر تسجيل ساعاتك، يجب التواصل مع HR");

            workLog.End = egyptTime;
            workLog.TotalTime = totalWorkTime;

            await _workLogRepository.UpdateAsync(workLog);
            await _monthlyDataRepository.AddHoursAsync(employeeId, totalWorkTime.TotalHours);

            return Result<bool>.Success(true);
        }

        public async Task<Result<PaginatedResponse<WorkLogDto>>> GetAllAsync(int employeeId, int page, int pageSize)
        {
            var workLogs = await _workLogRepository.GetAllAsync(employeeId, page, pageSize);
            var totalCount = await _workLogRepository.GetTotalCountAsync(employeeId);

            var dtos = workLogs.Select(w => new WorkLogDto
            {
                Id = w.Id,
                EmployeeId = w.EmployeeId,
                Day = w.Day,
                Start = w.Start,
                End = w.End,
                TotalHours = w.TotalTime.TotalHours
            }).ToList();

            return Result<PaginatedResponse<WorkLogDto>>.Success(new PaginatedResponse<WorkLogDto>
            {
                Data = dtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        public async Task<Result<WorkLogDto>> GetOpenShiftAsync(int employeeId)
        {
            var workLog = await _workLogRepository.GetOpenShiftAsync(employeeId);
            if (workLog is null)
                return Result<WorkLogDto>.Failure("لا يوجد شيفت مفتوح");

            return Result<WorkLogDto>.Success(new WorkLogDto
            {
                Id = workLog.Id,
                EmployeeId = workLog.EmployeeId,
                Day = workLog.Day,
                Start = workLog.Start,
                End = workLog.End,
                TotalHours = workLog.TotalTime.TotalHours
            });
        }
        public async Task<Result<IList<AttendanceReportDto>>> GetReportAsync(DateOnly fromDate, DateOnly toDate, int? employeeId, int? branchId)
        {
            var workLogs = await _workLogRepository.GetReportAsync(fromDate, toDate, employeeId, branchId);

            var result = new List<AttendanceReportDto>();
            foreach (var workLog in workLogs)
            {
                var employeeInfo = await _employeeRepository.GetEmployeeBasicInfoAsync(workLog.EmployeeId);
                if (employeeInfo is null) continue;

                if (branchId.HasValue && employeeInfo.Value.BranchId != branchId.Value)
                    continue;

                var branchInfo = await _branchRepository.GetBranchByIdAsync(employeeInfo.Value.BranchId);
                var schedule = await _scheduleRepository.GetEmployeeScheduleByDayAsync(workLog.EmployeeId, workLog.Day.DayOfWeek);

                result.Add(new AttendanceReportDto
                {
                    EmployeeId = workLog.EmployeeId,
                    EmployeeName = employeeInfo.Value.Name,
                    BranchName = branchInfo?.Name ?? string.Empty,
                    Day = workLog.Day,
                    ScheduledCheckIn = schedule?.CheckInTime ?? TimeOnly.MinValue,
                    ActualCheckIn = workLog.Start == TimeOnly.MinValue ? null : workLog.Start,
                    ScheduledCheckOut = schedule?.CheckOutTime ?? TimeOnly.MinValue,
                    ActualCheckOut = workLog.End == TimeOnly.MinValue ? null : workLog.End
                });
            }

            return Result<IList<AttendanceReportDto>>.Success(result);
        }
    }
}

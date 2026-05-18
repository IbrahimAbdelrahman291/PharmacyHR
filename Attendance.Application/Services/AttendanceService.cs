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
        private readonly IMonthlyDataRepository _monthlyDataRepository;

        public AttendanceService(
            IWorkLogRepository workLogRepository,
            IMonthlyDataRepository monthlyDataRepository)
        {
            _workLogRepository = workLogRepository;
            _monthlyDataRepository = monthlyDataRepository;
        }

        private readonly IEmployeeScheduleRepository _scheduleRepository;

        public AttendanceService(
            IWorkLogRepository workLogRepository,
            IMonthlyDataRepository monthlyDataRepository,
            IEmployeeScheduleRepository scheduleRepository)
        {
            _workLogRepository = workLogRepository;
            _monthlyDataRepository = monthlyDataRepository;
            _scheduleRepository = scheduleRepository;
        }

        public async Task<Result<bool>> StartShiftAsync(int employeeId)
        {
            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);
            var egyptDate = DateOnly.FromDateTime(egyptNow);
            var egyptTime = TimeOnly.FromDateTime(egyptNow);

            // تأكد إن اليوم موجود في الـ Schedule
            var schedule = await _scheduleRepository.GetScheduleByDayAsync(employeeId, egyptDate.DayOfWeek);
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
    }
}

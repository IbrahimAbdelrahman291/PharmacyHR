using Employees.Application.DTOs;
using Employees.Application.Interfaces;
using Employees.Domain.Entities;
using Employees.Domain.Interfaces;
using SharedKernel.Wrappers;

namespace Employees.Application.Services
{
    public class EmployeeScheduleService : IEmployeeScheduleService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeScheduleService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<Result<bool>> AddScheduleAsync(int employeeId, CreateEmployeeScheduleDto dto)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee is null)
                return Result<bool>.Failure("Employee not found");

            var existing = await _employeeRepository.GetScheduleByDayAsync(employeeId, dto.DayOfWeek);
            if (existing is not null)
                return Result<bool>.Failure("يوجد جدول بالفعل لهذا اليوم");

            var schedule = new EmployeeSchedule
            {
                EmployeeId = employeeId,
                DayOfWeek = dto.DayOfWeek,
                CheckInTime = dto.CheckInTime,
                CheckOutTime = dto.CheckOutTime
            };

            await _employeeRepository.AddScheduleAsync(schedule);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> UpdateScheduleAsync(int employeeId,int scheduleId, CreateEmployeeScheduleDto dto)
        {
            var schedules = await _employeeRepository.GetSchedulesByEmployeeIdAsync(employeeId);
            var schedule = schedules.FirstOrDefault(s => s.Id == scheduleId);
            if (schedule is null)
                return Result<bool>.Failure("Schedule not found");

            schedule.CheckInTime = dto.CheckInTime;
            schedule.CheckOutTime = dto.CheckOutTime;

            await _employeeRepository.UpdateScheduleAsync(schedule);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeleteScheduleAsync(int scheduleId)
        {
            var result = await _employeeRepository.DeleteScheduleAsync(scheduleId);
            if (!result)
                return Result<bool>.Failure("Schedule not found");
            return Result<bool>.Success(true);
        }

        public async Task<Result<IList<EmployeeScheduleDto>>> GetSchedulesByEmployeeIdAsync(int employeeId)
        {
            var schedules = await _employeeRepository.GetSchedulesByEmployeeIdAsync(employeeId);

            var dtos = schedules.Select(s => new EmployeeScheduleDto
            {
                Id = s.Id,
                EmployeeId = s.EmployeeId,
                DayOfWeek = s.DayOfWeek,
                CheckInTime = s.CheckInTime,
                CheckOutTime = s.CheckOutTime
            }).ToList();

            return Result<IList<EmployeeScheduleDto>>.Success(dtos);
        }
    }
}
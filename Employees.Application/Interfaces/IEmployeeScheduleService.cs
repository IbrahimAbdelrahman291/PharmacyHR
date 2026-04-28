using Employees.Application.DTOs;
using SharedKernel.Wrappers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Application.Interfaces
{
    public interface IEmployeeScheduleService
    {
        Task<Result<bool>> AddScheduleAsync(int employeeId, CreateEmployeeScheduleDto dto);
        Task<Result<bool>> UpdateScheduleAsync(int employeeId, int scheduleId, CreateEmployeeScheduleDto dto);
        Task<Result<bool>> DeleteScheduleAsync(int scheduleId);
        Task<Result<IList<EmployeeScheduleDto>>> GetSchedulesByEmployeeIdAsync(int employeeId);
    }
}

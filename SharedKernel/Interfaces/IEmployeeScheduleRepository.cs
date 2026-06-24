namespace SharedKernel.Interfaces
{
    public interface IEmployeeScheduleRepository
    {
        Task<(TimeOnly CheckInTime, TimeOnly CheckOutTime)?> GetEmployeeScheduleByDayAsync(int employeeId, DayOfWeek dayOfWeek);
        Task<IList<(int EmployeeId, TimeOnly CheckInTime, TimeOnly CheckOutTime)>> GetAllEmployeesWithScheduleByDayAsync(DayOfWeek dayOfWeek, int? employeeId = 0);
    }
}

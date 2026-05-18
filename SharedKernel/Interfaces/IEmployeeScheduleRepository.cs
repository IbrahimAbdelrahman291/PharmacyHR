namespace SharedKernel.Interfaces
{
    public interface IEmployeeScheduleRepository
    {
        Task<(TimeOnly CheckInTime, TimeOnly CheckOutTime)?> GetEmployeeScheduleByDayAsync(int employeeId, DayOfWeek dayOfWeek);
    }
}

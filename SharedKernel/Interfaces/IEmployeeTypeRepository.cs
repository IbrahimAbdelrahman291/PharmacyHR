namespace SharedKernel.Interfaces
{
    public interface IEmployeeTypeRepository
    {
        Task UpdateEmployeeTypeAsync(int employeeId, string type);
    }
}
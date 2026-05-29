

namespace SharedKernel.Interfaces
{
    public interface IBranchRepository
    {
        Task<(int Id, string Name)?> GetBranchByIdAsync(int id);
    }
}

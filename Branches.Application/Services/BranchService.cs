using Branches.Application.DTOs;
using Branches.Application.Interfaces;
using Branches.Domain.Entities;
using Branches.Domain.Interfaces;
using SharedKernel.Wrappers;

namespace Branches.Application.Services
{
    public class BranchService : IBranchService
    {
        private readonly IBranchRepository _repository;

        public BranchService(IBranchRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> AddAsync(CreateBranchDto dto)
        {
            var branch = new Branch 
            { 
                Name = dto.Name,
                TargetNumberOfEmployees = dto.TargetNumberOfEmployees,
                TargetSalaries = dto.TargetSalaries,
                TargetHours = dto.TargetHours,
            };
            await _repository.AddAsync(branch);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            var result = await _repository.DeleteAsync(id);
            if (!result)
                return Result<bool>.Failure("Branch not found");

            return Result<bool>.Success(true);
        }

        public async Task<Result<PaginatedResponse<BranchDto>>> GetAllAsync(int page, int pageSize)
        {
            var branches = await _repository.GetAllAsync(page, pageSize);
            var totalCount = await _repository.GetTotalCountAsync();

            var dtos = branches.Select(b => new BranchDto
            {
                Id = b.Id,
                Name = b.Name,
                TargetNumberOfEmployees = b.TargetNumberOfEmployees,
                TargetSalaries = b.TargetSalaries,
                TargetHours = b.TargetHours,
                
            }).ToList();

            return Result<PaginatedResponse<BranchDto>>.Success(new PaginatedResponse<BranchDto>
            {
                Data = dtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        public async Task<Result<bool>> UpdateAsync(int id, UpdateBranchDto dto)
        {
            var branch = await _repository.GetByIdAsync(id);
            if (branch is null)
                return Result<bool>.Failure("Branch not found");

            branch.TargetNumberOfEmployees = dto.TargetNumberOfEmployees ?? branch.TargetNumberOfEmployees;
            branch.TargetSalaries = dto.TargetSalaries ?? branch.TargetSalaries;
            branch.TargetHours = dto.TargetHours ?? branch.TargetHours;

            var result = await _repository.UpdateAsync(branch);
            if (!result)
                return Result<bool>.Failure("Failed to update branch");

            return Result<bool>.Success(true);
        }
    }
}
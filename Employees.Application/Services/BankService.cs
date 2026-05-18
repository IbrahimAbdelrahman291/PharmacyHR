using Employees.Application.DTOs;
using Employees.Application.Interfaces;
using Employees.Domain.Entities;
using SharedKernel.Wrappers;
using Employees.Domain.Interfaces;

namespace Employees.Application.Services
{
    public class BankService : IBankService
    {
        private readonly IEmployeeRepository _repository;

        public BankService(IEmployeeRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> AddAsync(CreateBankDto dto)
        {
            var bank = new Bank { Name = dto.Name };
            await _repository.AddBankAsync(bank);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            var result = await _repository.DeleteBankAsync(id);
            if (!result)
                return Result<bool>.Failure("Bank not found");
            return Result<bool>.Success(true);
        }

        public async Task<Result<IList<BankDto>>> GetAllAsync()
        {
            var banks = await _repository.GetAllBanksAsync();
            var dtos = banks.Select(b => new BankDto
            {
                Id = b.Id,
                Name = b.Name
            }).ToList();
            return Result<IList<BankDto>>.Success(dtos);
        }
    }
}

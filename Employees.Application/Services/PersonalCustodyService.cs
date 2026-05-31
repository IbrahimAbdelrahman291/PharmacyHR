using Employees.Application.DTOs;
using Employees.Application.Interfaces;
using Employees.Domain.Entities;
using Employees.Domain.Interfaces;
using SharedKernel.Wrappers;

namespace Employees.Application.Services
{
    public class PersonalCustodyService : IPersonalCustodyService
    {
        private readonly IEmployeeRepository _repository;

        public PersonalCustodyService(IEmployeeRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> AddAsync(int employeeId, CreatePersonalCustodyDto dto)
        {
            var custody = new PersonalCustody
            {
                EmployeeId = employeeId,
                Name = dto.Name,
                CreatedAt = DateTime.UtcNow
            };
            await _repository.AddCustodyAsync(custody);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            var result = await _repository.DeleteCustodyAsync(id);
            if (!result)
                return Result<bool>.Failure("Custody not found");
            return Result<bool>.Success(true);
        }

        public async Task<Result<IList<PersonalCustodyDto>>> GetByEmployeeIdAsync(int employeeId)
        {
            var custodies = await _repository.GetCustodiesByEmployeeIdAsync(employeeId);
            var dtos = custodies.Select(c => new PersonalCustodyDto
            {
                Id = c.Id,
                EmployeeId = c.EmployeeId,
                Name = c.Name,
                CreatedAt = c.CreatedAt
            }).ToList();
            return Result<IList<PersonalCustodyDto>>.Success(dtos);
        }
    }
}
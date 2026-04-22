using Employees.Application.DTOs;
using Employees.Application.Interfaces;
using Employees.Domain.Entities;
using Employees.Domain.Interfaces;
using SharedKernel.Wrappers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Application.Services
{
    public class EvaluationCriteriaService : IEvaluationCriteriaService
    {
        private readonly IEvaluationCriteriaRepository _repository;

        public EvaluationCriteriaService(IEvaluationCriteriaRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> AddAsync(CreateEvaluationCriteriaDto dto)
        {
            var criteria = new EvaluationCriteria
            {
                Name = dto.Name
            };

            await _repository.AddAsync(criteria);
            return Result<bool>.Success(true);
        }

        public async Task<Result<PaginatedResponse<EvaluationCriteriaDto>>> GetAllAsync(int page, int pageSize)
        {
            var criterias = await _repository.GetAllAsync(page, pageSize);
            var totalCount = await _repository.GetTotalCountAsync();

            var dtos = criterias.Select(c => new EvaluationCriteriaDto
            {
                Id = c.Id,
                Name = c.Name
            }).ToList();

            return Result<PaginatedResponse<EvaluationCriteriaDto>>.Success(new PaginatedResponse<EvaluationCriteriaDto>
            {
                Data = dtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            var result = await _repository.DeleteAsync(id);
            if (!result)
                return Result<bool>.Failure("Evaluation criteria not found");

            return Result<bool>.Success(true);
        }
    }
}

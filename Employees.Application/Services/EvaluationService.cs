using Employees.Application.DTOs;
using Employees.Application.Interfaces;
using Employees.Domain.Entities;
using Employees.Domain.Interfaces;
using SharedKernel.Wrappers;


namespace Employees.Application.Services
{
    public class EvaluationService : IEvaluationService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EvaluationService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<Result<bool>> AddEvaluationAsync(CreateEvaluationDto dto, string evaluatedBy)
        {
            var validQuarters = new[] { "Q1", "Q2", "Q3", "Q4" };
            if (!validQuarters.Contains(dto.Quarter))
                return Result<bool>.Failure("Invalid quarter");

            var validRatings = new[] { "ممتاز", "جيد", "ضعيف" };
            if (dto.Results.Any(r => !validRatings.Contains(r.Rating)))
                return Result<bool>.Failure("Invalid rating");

            var existing = await _employeeRepository.GetEvaluationByQuarterAsync(dto.EmployeeId, dto.Quarter, dto.Year);
            if (existing is not null)
                return Result<bool>.Failure("يوجد تقييم بالفعل لهذا الربع والسنة");

            var evaluation = new QuarterlyEvaluation
            {
                EmployeeId = dto.EmployeeId,
                EvaluatedBy = evaluatedBy,
                Quarter = dto.Quarter,
                Year = dto.Year,
                EvaluationResults = dto.Results.Select(r => new EvaluationResult
                {
                    EvaluationCriteriaId = r.EvaluationCriteriaId,
                    Rating = r.Rating
                }).ToList()
            };

            await _employeeRepository.AddEvaluationAsync(evaluation);
            return Result<bool>.Success(true);
        }

        public async Task<Result<IList<EvaluationDto>>> GetEvaluationsAsync(int employeeId)
        {
            var evaluations = await _employeeRepository.GetEvaluationsAsync(employeeId);

            var dtos = evaluations.Select(e => new EvaluationDto
            {
                Id = e.Id,
                EmployeeId = e.EmployeeId,
                EvaluatedBy = e.EvaluatedBy,
                Quarter = e.Quarter,
                Year = e.Year,
                Results = e.EvaluationResults.Select(r => new EvaluationResultDto
                {
                    EvaluationCriteriaId = r.EvaluationCriteriaId,
                    Rating = r.Rating
                }).ToList()
            }).ToList();

            return Result<IList<EvaluationDto>>.Success(dtos);
        }
    }
}

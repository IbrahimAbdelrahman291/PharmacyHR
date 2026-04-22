using Employees.Domain.Entities;
using Employees.Domain.Interfaces;
using Employees.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace Employees.Infrastructure.Repositories
{
    public class EvaluationCriteriaRepository : IEvaluationCriteriaRepository
    {
        private readonly EmployeesDbContext _context;

        public EvaluationCriteriaRepository(EmployeesDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(EvaluationCriteria criteria)
        {
            await _context.EvaluationCriterias.AddAsync(criteria);
            await _context.SaveChangesAsync();
        }

        public async Task<IList<EvaluationCriteria>> GetAllAsync(int page, int pageSize)
        {
            return await _context.EvaluationCriterias
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
            => await _context.EvaluationCriterias.CountAsync();

        public async Task<bool> DeleteAsync(int id)
        {
            var criteria = await _context.EvaluationCriterias.FindAsync(id);
            if (criteria is null) return false;

            _context.EvaluationCriterias.Remove(criteria);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

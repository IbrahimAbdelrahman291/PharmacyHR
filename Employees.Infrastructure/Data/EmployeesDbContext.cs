using Employees.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Infrastructure.Data
{
    public class EmployeesDbContext : DbContext
    {
        public EmployeesDbContext(DbContextOptions<EmployeesDbContext> options) : base(options)
        {
        }

        public DbSet<EvaluationCriteria> EvaluationCriterias { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeHistory> EmployeeHistories { get; set; }
    }
}

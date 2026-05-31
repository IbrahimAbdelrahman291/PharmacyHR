using Employees.Domain.Interfaces;
using Employees.Infrastructure.Data;
using Employees.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddEmployeesInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<EmployeesDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IEvaluationCriteriaRepository, EvaluationCriteriaRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<SharedKernel.Interfaces.IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<SharedKernel.Interfaces.IEmployeeScheduleRepository, EmployeeRepository>();
            services.AddScoped<SharedKernel.Interfaces.IEmployeeTypeRepository, EmployeeRepository>();

            return services;
        }
    }
}

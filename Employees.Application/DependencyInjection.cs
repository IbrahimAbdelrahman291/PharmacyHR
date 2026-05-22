using Employees.Application.Interfaces;
using Employees.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddEmployeesApplication(
            this IServiceCollection services)
        {
            services.AddScoped<IEvaluationCriteriaService, EvaluationCriteriaService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IEmployeeScheduleService, EmployeeScheduleService>();
            services.AddScoped<IBankService, BankService>();
            services.AddScoped<IEvaluationService, EvaluationService>();


            return services;
        }
    }
}

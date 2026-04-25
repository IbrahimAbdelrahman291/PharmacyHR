using Microsoft.Extensions.DependencyInjection;
using Payroll.Application.Interfaces;
using Payroll.Application.Services;

namespace Payroll.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPayrollApplication(
            this IServiceCollection services)
        {
            services.AddScoped<IPayrollService, PayrollService>();
            return services;
        }
    }
}

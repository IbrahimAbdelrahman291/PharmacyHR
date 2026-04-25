using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payroll.Application.Interfaces;
using Payroll.Domain.Interfaces;
using Payroll.Infrastructure.Data;
using Payroll.Infrastructure.Jobs;
using Payroll.Infrastructure.Repositories;


namespace Payroll.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPayrollInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<PayrollDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IMonthlyDataRepository, MonthlyDataRepository>();
            services.AddScoped<SharedKernel.Interfaces.IMonthlyDataRepository, MonthlyDataRepository>();
            services.AddScoped<INewMonthJob, NewMonthJob>();

            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection")));

            services.AddHangfireServer();

            return services;
        }
    }
}

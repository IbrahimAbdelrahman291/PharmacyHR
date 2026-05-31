using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Requests.Domain.Interfaces;
using Requests.Infrastructure.Data;
using Requests.Infrastructure.Jobs;
using Requests.Infrastructure.Repositories;
using SharedKernel.Interfaces;

namespace Requests.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRequestsInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<RequestsDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IComplaintRepository, ComplaintRepository>();
            services.AddScoped<IForgetedHoursRepository, ForgetedHoursRepository>();
            services.AddScoped<IHolidayRepository, HolidayRepository>();
            services.AddScoped<IBorrowRepository, BorrowRepository>();
            services.AddScoped<IInstallmentBorrowJob, InstallmentBorrowJob>();
            services.AddScoped<IOvertimeRepository, OvertimeRepository>();
            services.AddScoped<IResignationRepository, ResignationRepository>();
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            return services;
        }
    }
}

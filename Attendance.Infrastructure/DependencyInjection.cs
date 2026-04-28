using Attendance.Domain.Interfaces;
using Attendance.Infrastructure.Data;
using Attendance.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Attendance.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAttendanceInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<AttendanceDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IWorkLogRepository, WorkLogRepository>();

            return services;
        }
    }
}

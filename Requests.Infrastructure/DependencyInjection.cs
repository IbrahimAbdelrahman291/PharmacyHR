using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Requests.Domain.Interfaces;
using Requests.Infrastructure.Data;
using Requests.Infrastructure.Repositories;

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

            return services;
        }
    }
}

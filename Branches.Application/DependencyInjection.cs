using Branches.Application.Interfaces;
using Branches.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Branches.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBranchesApplication(
            this IServiceCollection services)
        {
            services.AddScoped<IBranchService, BranchService>();
            return services;
        }
    }
}
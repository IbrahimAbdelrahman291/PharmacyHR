using Branches.Domain.Interfaces;
using Branches.Infrastructure.Data;
using Branches.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Branches.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBranchesInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<BranchesDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IBranchRepository, BranchRepository>();
            services.AddScoped<SharedKernel.Interfaces.IBranchRepository, BranchRepository>();


            return services;
        }
    }
}

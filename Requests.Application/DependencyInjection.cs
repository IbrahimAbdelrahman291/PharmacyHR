using Microsoft.Extensions.DependencyInjection;
using Requests.Application.DTOs;
using Requests.Application.Interfaces;
using Requests.Application.Services;

namespace Requests.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRequestsApplication(
            this IServiceCollection services)
        {
            services.AddScoped<IComplaintService, ComplaintService>();
            services.AddScoped<IForgetedHoursService, ForgetedHoursService>();
            services.AddScoped<IHolidayService, HolidayService>();
            return services;
        }
    }
}

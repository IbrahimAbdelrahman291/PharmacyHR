using Attendance.Application.Interfaces;
using Attendance.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Attendance.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAttendanceApplication(
            this IServiceCollection services)
        {
            services.AddScoped<IAttendanceService, AttendanceService>();
            return services;
        }
    }
}

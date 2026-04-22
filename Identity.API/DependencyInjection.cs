using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddIdentityModule(
            this IServiceCollection services)
        {
            return services;
        }
    }
}

using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Infrastructure.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Seed Roles
            var roles = new[]
            {
                UserRoles.Admin,
                UserRoles.HR,
                UserRoles.Accountant,
                UserRoles.Employee,
                UserRoles.Control,
                UserRoles.Manager,
                UserRoles.AreaManager

            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed Admin User
            if (await userManager.FindByNameAsync("admin") is null)
            {
                var admin = new User
                {
                    UserName = "Admin1",
                    Name = "Admin"
                };

                await userManager.CreateAsync(admin, "P@$$w0rd");
                await userManager.AddToRoleAsync(admin, UserRoles.Admin);
            }
        }
    }
}

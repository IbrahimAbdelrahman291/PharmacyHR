using Branches.Application;
using Branches.Infrastructure;
using Branches.Infrastructure.Data;
using Employees.Application;
using Employees.Infrastructure;
using Employees.Infrastructure.Data;
using Hangfire;
using Identity.Application;
using Identity.Infrastructure;
using Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Payroll.Application;
using Payroll.Application.Interfaces;
using Payroll.Infrastructure;
using Payroll.Infrastructure.Data;
using System.Text;
using Attendance.Application;
using Attendance.Infrastructure;

public partial class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Controllers
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(Identity.API.Controllers.AuthController).Assembly)
            .AddApplicationPart(typeof(Employees.API.Controllers.EvaluationCriteriaController).Assembly)
            .AddApplicationPart(typeof(Branches.API.Controllers.BranchesController).Assembly)
            .AddApplicationPart(typeof(Employees.API.Controllers.EmployeesController).Assembly)
            .AddApplicationPart(typeof(Payroll.API.Controllers.PayrollController).Assembly)
            .AddApplicationPart(typeof(Attendance.API.Controllers.AttendanceController).Assembly);



        // Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Identity Module
        builder.Services.AddIdentityInfrastructure(builder.Configuration);
        builder.Services.AddIdentityApplication();

        // Employees Module
        builder.Services.AddEmployeesInfrastructure(builder.Configuration);
        builder.Services.AddEmployeesApplication();

        // Branches Module
        builder.Services.AddBranchesInfrastructure(builder.Configuration);
        builder.Services.AddBranchesApplication();
        // Payroll Module
        builder.Services.AddPayrollInfrastructure(builder.Configuration);
        builder.Services.AddPayrollApplication();

        // Attendance Module
        builder.Services.AddAttendanceInfrastructure(builder.Configuration);
        builder.Services.AddAttendanceApplication();

        // JWT Authentication
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
            };
        });

        builder.Services.AddAuthorization();

        // cors 
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin();
            });
        });

        var app = builder.Build();

        #region migrations
        // Migrate databases and seed identity data
        //using (var scope = app.Services.CreateScope())
        //{
        //    try
        //    {
        //        var identityDb = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        //        await identityDb.Database.MigrateAsync();

        //        var employeesDb = scope.ServiceProvider.GetRequiredService<EmployeesDbContext>();
        //        await employeesDb.Database.MigrateAsync();

        //        var branchesDb = scope.ServiceProvider.GetRequiredService<BranchesDbContext>();
        //        await branchesDb.Database.MigrateAsync();

        //        var payrollDb = scope.ServiceProvider.GetRequiredService<PayrollDbContext>();
        //        await payrollDb.Database.MigrateAsync();

        //        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Identity.Domain.Entities.User>>();
        //        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        //        await IdentitySeeder.SeedAsync(userManager, roleManager);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Migration error: {ex.Message}");
        //    }
        //} 
        #endregion

        //Hangfire
        app.UseHangfireDashboard("/hangfire");

        var recurringOptions = new RecurringJobOptions
        {
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time")
        };

        RecurringJob.AddOrUpdate<INewMonthJob>(
            "start-new-month-job",
            service => service.ExecuteAsync(),
            "0 0 1 * *",
            recurringOptions
        );


        // Seed Identity Data
        #region Data seeding Identity
        using (var scope = app.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Identity.Domain.Entities.User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await IdentitySeeder.SeedAsync(userManager, roleManager);
        }
        #endregion

        
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
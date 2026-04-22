using Identity.Application;
using Identity.Infrastructure;
using Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Employees.Application;
using Employees.Infrastructure;
using Branches.Application;
using Branches.Infrastructure;

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
            .AddApplicationPart(typeof(Employees.API.Controllers.EmployeesController).Assembly);


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
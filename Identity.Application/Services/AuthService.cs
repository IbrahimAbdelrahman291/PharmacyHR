using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Enums;
using Identity.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Wrappers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IAuthRepository authRepository, IConfiguration configuration)
        {
            _authRepository = authRepository;
            _configuration = configuration;
        }

        public async Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request)
        {
            var user = await _authRepository.FindByUsernameAsync(request.Username);
            if (user is null)
                return Result<LoginResponseDto>.Failure("Invalid username or password");

            var isPasswordValid = await _authRepository.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
                return Result<LoginResponseDto>.Failure("Invalid email or password");

            var roles = await _authRepository.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? string.Empty;

            var token = GenerateToken(user, role);

            return Result<LoginResponseDto>.Success(new LoginResponseDto
            {
                Token = token,
                Name = user.Name,
                Role = role
            });
        }

        private string GenerateToken(Domain.Entities.User user, string role)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.Name),
                new(ClaimTypes.Role, role)
            };

            if (role == UserRoles.Employee)
            {
                claims.Add(new Claim("EmployeeId", user.EmployeeId?.ToString() ?? string.Empty));
                claims.Add(new Claim("BranchId", user.BranchId?.ToString() ?? string.Empty));
            }

            if (role == UserRoles.Manager)
            {
                claims.Add(new Claim("BranchId", user.BranchId?.ToString() ?? string.Empty));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

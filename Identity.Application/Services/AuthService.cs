using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
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
            if (!user.IsActive)
                return Result<LoginResponseDto>.Failure("Account is disabled");
            var isPasswordValid = await _authRepository.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
                return Result<LoginResponseDto>.Failure("Invalid username or password");

            var roles = await _authRepository.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? string.Empty;

            var token = await GenerateTokenAsync(user, role);

            return Result<LoginResponseDto>.Success(new LoginResponseDto
            {
                Token = token,
                Name = user.Name,
                Role = role
            });
        }

        public async Task<Result<bool>> CreateUserAsync(CreateUserDto dto)
        {
            var existingUser = await _authRepository.FindByUsernameAsync(dto.Username);
            if (existingUser is not null)
                return Result<bool>.Failure("Username already exists");

            var validRoles = new[]
            {
                UserRoles.Admin,
                UserRoles.HR,
                UserRoles.Accountant,
                UserRoles.Control,
                UserRoles.Manager,
                UserRoles.AreaManager,
                UserRoles.CEO
            };

            if (!validRoles.Contains(dto.Role))
                return Result<bool>.Failure("Invalid role");

            var user = new User
            {
                UserName = dto.Username,
                Name = dto.Name
            };

            var result = await _authRepository.CreateUserAsync(user, dto.Password, dto.Role);
            if (!result)
                return Result<bool>.Failure("Failed to create user");

            // لو AreaManager ضيف الفروع
            if (dto.Role == UserRoles.AreaManager && dto.BranchIds is not null && dto.BranchIds.Any())
                await _authRepository.AddAreaManagerBranchesAsync(user.Id, dto.BranchIds);

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await _authRepository.FindByIdAsync(userId);
            if (user is null)
                return Result<bool>.Failure("User not found");

            var result = await _authRepository.ChangePasswordAsync(user, dto.NewPassword);
            if (!result)
                return Result<bool>.Failure("Failed to change password");

            return Result<bool>.Success(true);
        }

        private async Task<string> GenerateTokenAsync(User user, string role)
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
            if (role == UserRoles.AreaManager)
            {
                var branchIds = await _authRepository.GetAreaManagerBranchesAsync(user.Id);
                claims.Add(new Claim("BranchIds", string.Join(",", branchIds)));
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

        public async Task<Result<PaginatedResponse<UserDto>>> GetAllUsersAsync(int page, int pageSize,string? name)
        {
            var users = await _authRepository.GetAllUsersAsync(page, pageSize,name);
            var totalCount = await _authRepository.GetTotalUsersCountAsync();

            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                var roles = await _authRepository.GetRolesAsync(user);
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    Username = user.UserName ?? string.Empty,
                    Name = user.Name,
                    Role = roles.FirstOrDefault() ?? string.Empty,
                    IsActive = user.IsActive,
                });
            }

            return Result<PaginatedResponse<UserDto>>.Success(new PaginatedResponse<UserDto>
            {
                Data = userDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }
        public async Task<Result<bool>> ToggleUserAsync(string userId)
        {
            var result = await _authRepository.ToggleUserAsync(userId);
            if (!result)
                return Result<bool>.Failure("User not found");
            return Result<bool>.Success(true);
        }
        public async Task<Result<IList<int>>> GetAreaManagerBranchesAsync(string userId)
        {
            var branches = await _authRepository.GetAreaManagerBranchesAsync(userId);
            return Result<IList<int>>.Success(branches);
        }

        public async Task<Result<bool>> AddAreaManagerBranchAsync(string userId, int branchId)
        {
            var result = await _authRepository.AddAreaManagerBranchAsync(userId, branchId);
            if (!result)
                return Result<bool>.Failure("Branch already exists for this Area Manager");
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> RemoveAreaManagerBranchAsync(string userId, int branchId)
        {
            var result = await _authRepository.RemoveAreaManagerBranchAsync(userId, branchId);
            if (!result)
                return Result<bool>.Failure("Branch not found for this Area Manager");
            return Result<bool>.Success(true);
        }
    }
}

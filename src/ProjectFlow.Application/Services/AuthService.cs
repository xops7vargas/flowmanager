using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ProjectFlow.Application.DTOs;
using ProjectFlow.Application.Interfaces;
using ProjectFlow.Domain.Entities;
using ProjectFlow.Domain.Interfaces;

namespace ProjectFlow.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account has not been activated. Please contact the administrator.");

        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            User = MapToDto(user)
        };
    }

    public async Task<AuthResponseDto> RegisterAsync(CreateUserDto dto)
    {
        var existing = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
        if (existing != null)
            throw new InvalidOperationException("Email already exists");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        throw new UnauthorizedAccessException("Registration pending. Please contact administrator to activate your account.");
    }

    public Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
    {
        throw new NotImplementedException("Implement token refresh logic");
    }

    public Task RevokeTokenAsync(Guid userId)
    {
        throw new NotImplementedException("Implement token revocation");
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found");
        return MapToDto(user);
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"] ?? "DefaultSecretKey12345678901234567890"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
        };

        foreach (var role in user.UserRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Role.Name));
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "ProjectFlow",
            audience: _configuration["Jwt:Audience"] ?? "ProjectFlowAPI",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "15")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }

    private UserDto MapToDto(User user)
    {
        var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
        var permissions = new List<string>();
        
        if (user.UserRoles.Any())
        {
            var rolePermissions = _unitOfWork.RolePermissions.GetAllAsync().Result
                .Where(rp => roleIds.Contains(rp.RoleId))
                .ToList();
            var permissionIds = rolePermissions.Select(rp => rp.PermissionId).ToList();
            permissions = _unitOfWork.Permissions.GetAllAsync().Result
                .Where(p => permissionIds.Contains(p.Id))
                .Select(p => p.Name)
                .ToList();
        }
        
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Avatar = user.Avatar,
            IsActive = user.IsActive,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            Permissions = permissions,
            CreatedAt = user.CreatedAt
        };
    }
}
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using AcademicSystem.Application.Common.Interfaces;
using AcademicSystem.Application.DTOs.Auth;
using AcademicSystem.Application.Common.Interfaces.Repositories;
using AcademicSystem.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AcademicSystem.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IPasswordHasher passwordHasher, IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            // find user by email
            // Note: assumes User.Email is unique
            var users = await _userRepository.GetAllAsync();
            var user = System.Linq.Enumerable.FirstOrDefault(users, u => u.Email.ToLower() == request.Email.ToLower());
            if (user == null) throw new Exception("Invalid credentials");
            if (!_passwordHasher.Verify(user.PasswordHash ?? string.Empty, request.Password)) throw new Exception("Invalid credentials");

            var token = GenerateJwtToken(user);
            var refreshRaw = await CreateRefreshTokenAsync(user);
            // persist refresh token
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponseDto { Token = token, RefreshToken = refreshRaw };
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterUserDto request)
        {
            // create user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = _passwordHasher.Hash(request.Password),
                Role = 0
            };
            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            var token = GenerateJwtToken(user);
            var refreshRaw = await CreateRefreshTokenAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return new AuthResponseDto { Token = token, RefreshToken = refreshRaw };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string token)
        {
            // find refresh token by token hash - in this implementation we store token hash, so find by TokenHash
            var tokens = await _refreshTokenRepository.GetAllAsync();
            var rt = System.Linq.Enumerable.FirstOrDefault(tokens, t => !t.IsRevoked && _passwordHasher.Verify(t.TokenHash, token));
            if (rt == null) throw new Exception("Invalid refresh token");
            if (rt.IsRevoked || rt.ExpiresAt <= DateTime.UtcNow) throw new Exception("Expired token");

            var user = await _userRepository.GetByIdAsync(rt.UserId);
            if (user == null) throw new Exception("User not found");

            var jwt = GenerateJwtToken(user);
            var newRt = await CreateRefreshTokenAsync(user);
            // revoke old
            rt.IsRevoked = true;
            rt.RevokedAt = DateTime.UtcNow;
            _refreshTokenRepository.Update(rt);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponseDto { Token = jwt, RefreshToken = newRt }; // newRt is raw token
        }

        private string GenerateJwtToken(User user)
        {
            var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured");
            var issuer = _configuration["Jwt:Issuer"] ?? "local";
            var audience = _configuration["Jwt:Audience"] ?? "local";
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Map numeric role to a role name for authorization policies
            string roleName = user.Role switch
            {
                1 => "Admin",
                2 => "Instructor",
                _ => "User"
            };

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("role", user.Role.ToString()), // numeric role
                new Claim(System.Security.Claims.ClaimTypes.Role, roleName), // role name for Authorize
                new Claim("roleName", roleName)
            };

            var token = new JwtSecurityToken(issuer, audience, claims, expires: DateTime.UtcNow.AddMinutes(60), signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<string> CreateRefreshTokenAsync(User user)
        {
            // Generate a secure random token and store only its hash
            var rawTokenBytes = RandomNumberGenerator.GetBytes(64);
            var rawToken = Convert.ToBase64String(rawTokenBytes);
            var tokenHash = _passwordHasher.Hash(rawToken);

            var rt = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedByIp = "127.0.0.1",
                IsRevoked = false
            };
            await _refreshTokenRepository.AddAsync(rt);
            return rawToken; // return raw token to client (store only hash)
        }
    }
}
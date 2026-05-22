using System.Threading.Tasks;
using AcademicSystem.Application.DTOs.Auth;

namespace AcademicSystem.Application.Common.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task<AuthResponseDto> RegisterAsync(RegisterUserDto request);
        Task<AuthResponseDto> RefreshTokenAsync(string token);
    }
}
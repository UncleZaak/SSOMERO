using System.Threading.Tasks;
using Ssomero.Models;

namespace Ssomero.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(string email, string password);
    Task<bool> RegisterAsync(RegisterDto dto);
    Task<bool> RegisterStudentAsync(StudentRegisterDto dto);
    Task<bool> RegisterLecturerAsync(LecturerRegisterDto dto);
    Task<bool> SendOtpAsync(string email);
    Task<string?> VerifyOtpAsync(string email, string otpCode);
    Task<bool> TryRefreshTokenAsync();
    Task LogoutAsync();

    // Password reset flow
    Task<bool> ForgotPasswordAsync(string email);
    Task<string?> VerifyResetOtpAsync(string email, string otpCode);
    Task<bool> ResetPasswordAsync(string email, string resetToken, string newPassword);
}
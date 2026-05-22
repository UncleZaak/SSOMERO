using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Interfaces;
using Ssomero.ViewModels;

namespace Ssomero.ViewModels.UnitTests;

/// <summary>
/// Unit tests for <see cref="ForgotPasswordViewModel"/> and <see cref="ResetPasswordViewModel"/>.
/// All tests run without a MAUI host — no UI thread or XAML required.
/// </summary>
[TestClass]
public class ForgotPasswordViewModelTests
{
    private static (ForgotPasswordViewModel vm, Mock<IAuthService> auth) CreateForgot()
    {
        var auth = new Mock<IAuthService>(MockBehavior.Loose);
        var vm   = new ForgotPasswordViewModel(auth.Object, NullLogger<ForgotPasswordViewModel>.Instance);
        return (vm, auth);
    }

    // ── initial state ─────────────────────────────────────────────────────────

    [TestMethod]
    public void Constructor_InitialStep_IsStep1()
    {
        var (vm, _) = CreateForgot();
        Assert.IsTrue(vm.IsStep1);
        Assert.IsFalse(vm.IsStep2);
    }

    [TestMethod]
    public void Constructor_IsOtpSent_IsFalse()
    {
        var (vm, _) = CreateForgot();
        Assert.IsFalse(vm.IsOtpSent);
    }

    [TestMethod]
    public void Constructor_CanResendOtp_IsFalse()
    {
        var (vm, _) = CreateForgot();
        Assert.IsFalse(vm.CanResendOtp);
    }

    // ── SendOtp ───────────────────────────────────────────────────────────────

    [TestMethod]
    public async Task SendOtp_EmptyEmail_SetsErrorMessage()
    {
        var (vm, _) = CreateForgot();
        vm.Email = string.Empty;

        await vm.SendOtpCommand.ExecuteAsync(null);

        Assert.IsFalse(string.IsNullOrEmpty(vm.ErrorMessage));
        Assert.IsTrue(vm.IsStep1);
    }

    [TestMethod]
    public async Task SendOtp_ValidEmail_AdvancesToStep2()
    {
        var (vm, auth) = CreateForgot();
        auth.Setup(a => a.ForgotPasswordAsync(It.IsAny<string>())).ReturnsAsync(true);
        vm.Email = "user@example.com";

        await vm.SendOtpCommand.ExecuteAsync(null);

        Assert.IsTrue(vm.IsStep2);
        Assert.IsTrue(vm.IsOtpSent);
    }

    [TestMethod]
    public async Task SendOtp_ValidEmail_SetsIsBusyFalseAfterCompletion()
    {
        var (vm, auth) = CreateForgot();
        auth.Setup(a => a.ForgotPasswordAsync(It.IsAny<string>())).ReturnsAsync(true);
        vm.Email = "user@example.com";

        await vm.SendOtpCommand.ExecuteAsync(null);

        Assert.IsFalse(vm.IsBusy);
    }

    [TestMethod]
    public async Task SendOtp_ServiceThrows_SetsErrorMessage()
    {
        var (vm, auth) = CreateForgot();
        auth.Setup(a => a.ForgotPasswordAsync(It.IsAny<string>()))
            .ThrowsAsync(new HttpRequestException("timeout"));
        vm.Email = "user@example.com";

        await vm.SendOtpCommand.ExecuteAsync(null);

        Assert.IsFalse(string.IsNullOrEmpty(vm.ErrorMessage));
        Assert.IsTrue(vm.IsStep1, "Should remain on step 1 after failure");
        Assert.IsFalse(vm.IsBusy);
    }

    // ── VerifyOtp ─────────────────────────────────────────────────────────────

    [TestMethod]
    public async Task VerifyOtp_EmptyOtpCode_SetsErrorMessage()
    {
        var (vm, _) = CreateForgot();
        vm.Email   = "user@example.com";
        vm.OtpCode = string.Empty;

        await vm.VerifyOtpCommand.ExecuteAsync(null);

        Assert.IsFalse(string.IsNullOrEmpty(vm.ErrorMessage));
    }

    [TestMethod]
    public async Task VerifyOtp_ServiceReturnsNull_SetsErrorMessage()
    {
        var (vm, auth) = CreateForgot();
        auth.Setup(a => a.VerifyResetOtpAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string?)null);
        vm.Email   = "user@example.com";
        vm.OtpCode = "123456";

        await vm.VerifyOtpCommand.ExecuteAsync(null);

        Assert.IsFalse(string.IsNullOrEmpty(vm.ErrorMessage));
        Assert.IsFalse(vm.IsBusy);
    }

    [TestMethod]
    public async Task VerifyOtp_SetsBusyFalse_AfterCompletion()
    {
        var (vm, auth) = CreateForgot();
        auth.Setup(a => a.VerifyResetOtpAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string?)null); // navigate would fail in test, use null path
        vm.Email   = "user@example.com";
        vm.OtpCode = "123456";

        await vm.VerifyOtpCommand.ExecuteAsync(null);

        Assert.IsFalse(vm.IsBusy);
    }

    // ── ResendOtp ─────────────────────────────────────────────────────────────

    [TestMethod]
    public async Task ResendOtp_WhenOnCooldown_CommandCannotExecute()
    {
        var (vm, auth) = CreateForgot();
        auth.Setup(a => a.ForgotPasswordAsync(It.IsAny<string>())).ReturnsAsync(true);
        vm.Email = "user@example.com";
        await vm.SendOtpCommand.ExecuteAsync(null); // triggers cooldown

        // CanResendOtp is false while cooldown > 0
        Assert.IsFalse(vm.CanResendOtp);
    }

    [TestMethod]
    public void OtpCooldownText_ZeroSeconds_ShowsResendLabel()
    {
        var (vm, _) = CreateForgot();
        // OtpCooldownSeconds is 0 by default
        Assert.AreEqual("Resend OTP", vm.OtpCooldownText);
    }
}

[TestClass]
public class ResetPasswordViewModelTests
{
    private static (ResetPasswordViewModel vm, Mock<IAuthService> auth) CreateReset()
    {
        var auth = new Mock<IAuthService>(MockBehavior.Loose);
        var vm   = new ResetPasswordViewModel(auth.Object, NullLogger<ResetPasswordViewModel>.Instance);
        return (vm, auth);
    }

    // ── initial state ─────────────────────────────────────────────────────────

    [TestMethod]
    public void Constructor_PasswordVisibility_DefaultHidden()
    {
        var (vm, _) = CreateReset();
        Assert.IsTrue(vm.IsPasswordHidden);
        Assert.IsTrue(vm.IsConfirmPasswordHidden);
    }

    [TestMethod]
    public void Constructor_DoPasswordsMatch_IsFalse()
    {
        var (vm, _) = CreateReset();
        Assert.IsFalse(vm.DoPasswordsMatch);
    }

    // ── password toggle ───────────────────────────────────────────────────────

    [TestMethod]
    public void TogglePassword_FlipsVisibility()
    {
        var (vm, _) = CreateReset();
        vm.TogglePasswordCommand.Execute(null);
        Assert.IsFalse(vm.IsPasswordHidden);
        vm.TogglePasswordCommand.Execute(null);
        Assert.IsTrue(vm.IsPasswordHidden);
    }

    [TestMethod]
    public void ToggleConfirmPassword_FlipsVisibility()
    {
        var (vm, _) = CreateReset();
        vm.ToggleConfirmPasswordCommand.Execute(null);
        Assert.IsFalse(vm.IsConfirmPasswordHidden);
    }

    // ── validation ────────────────────────────────────────────────────────────

    [TestMethod]
    public async Task ResetPassword_EmptyFields_SetsErrorMessage()
    {
        var (vm, _) = CreateReset();
        await vm.ResetPasswordCommand.ExecuteAsync(null);
        Assert.IsFalse(string.IsNullOrEmpty(vm.ErrorMessage));
    }

    [TestMethod]
    public async Task ResetPassword_ShortPassword_SetsErrorMessage()
    {
        var (vm, _) = CreateReset();
        vm.NewPassword      = "abc";
        vm.ConfirmPassword  = "abc";

        await vm.ResetPasswordCommand.ExecuteAsync(null);

        Assert.IsFalse(string.IsNullOrEmpty(vm.ErrorMessage));
        Assert.IsTrue(vm.ErrorMessage.Contains("8"));
    }

    [TestMethod]
    public async Task ResetPassword_MismatchedPasswords_SetsErrorMessage()
    {
        var (vm, _) = CreateReset();
        vm.NewPassword     = "Password1!";
        vm.ConfirmPassword = "Different1!";

        await vm.ResetPasswordCommand.ExecuteAsync(null);

        Assert.IsTrue(vm.ErrorMessage.Contains("match", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void DoPasswordsMatch_SamePasswords_IsTrue()
    {
        var (vm, _) = CreateReset();
        vm.NewPassword     = "Password1!";
        vm.ConfirmPassword = "Password1!";
        Assert.IsTrue(vm.DoPasswordsMatch);
    }

    [TestMethod]
    public void DoPasswordsMatch_DifferentPasswords_IsFalse()
    {
        var (vm, _) = CreateReset();
        vm.NewPassword     = "Password1!";
        vm.ConfirmPassword = "Mismatch1!";
        Assert.IsFalse(vm.DoPasswordsMatch);
    }

    // ── strength hint ─────────────────────────────────────────────────────────

    [TestMethod]
    public void PasswordStrengthHint_EmptyPassword_IsEmpty()
    {
        var (vm, _) = CreateReset();
        Assert.AreEqual(string.Empty, vm.PasswordStrengthHint);
    }

    [TestMethod]
    public void PasswordStrengthHint_ShortPassword_ShowsTooShort()
    {
        var (vm, _) = CreateReset();
        vm.NewPassword = "abc";
        Assert.IsTrue(vm.PasswordStrengthHint.Contains("short", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void PasswordStrengthHint_StrongPassword_ShowsStrong()
    {
        var (vm, _) = CreateReset();
        vm.NewPassword = "Strong1!";
        Assert.IsTrue(vm.PasswordStrengthHint.Contains("Strong", StringComparison.OrdinalIgnoreCase));
    }

    // ── service failure ───────────────────────────────────────────────────────

    [TestMethod]
    public async Task ResetPassword_ServiceThrows_SetsErrorMessage()
    {
        var (vm, auth) = CreateReset();
        auth.Setup(a => a.ResetPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new HttpRequestException("Invalid or expired reset token."));
        vm.ResetToken      = "sometoken";
        vm.Email           = "user@example.com";
        vm.NewPassword     = "Strong1!";
        vm.ConfirmPassword = "Strong1!";

        await vm.ResetPasswordCommand.ExecuteAsync(null);

        Assert.IsFalse(string.IsNullOrEmpty(vm.ErrorMessage));
        Assert.IsFalse(vm.IsBusy);
    }

    [TestMethod]
    public async Task ResetPassword_SetsBusyFalse_AfterFailure()
    {
        var (vm, auth) = CreateReset();
        auth.Setup(a => a.ResetPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new HttpRequestException("error"));
        vm.ResetToken      = "tok";
        vm.Email           = "user@example.com";
        vm.NewPassword     = "Strong1!";
        vm.ConfirmPassword = "Strong1!";

        await vm.ResetPasswordCommand.ExecuteAsync(null);

        Assert.IsFalse(vm.IsBusy);
    }
}

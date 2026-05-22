using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssomero.Api.Dtos;
using Ssomero.Api.Services.Interfaces;

namespace Ssomero.Api.Controllers;

/// <summary>
/// Authenticated user profile endpoints.
/// All routes require a valid JWT — role is read from the token, not from a query parameter,
/// so the user can never elevate themselves by passing a different role.
/// </summary>
[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        IProfileService profileService,
        IFileStorageService fileStorage,
        ILogger<ProfileController> logger)
    {
        _profileService = profileService;
        _fileStorage    = fileStorage;
        _logger         = logger;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("sub claim missing"));

    private string GetRole() =>
        User.FindFirstValue(ClaimTypes.Role)
            ?? throw new InvalidOperationException("role claim missing");

    // ── GET /api/profile ─────────────────────────────────────────────────────

    /// <summary>Returns the authenticated user's role-specific profile.</summary>
    [HttpGet]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var userId = GetUserId();
        var role   = GetRole();

        var profile = await _profileService.GetProfileAsync(userId, role, ct);
        if (profile is null)
        {
            _logger.LogWarning("Profile not found for {UserId} ({Role})", userId, role);
            return NotFound(new { error = "Profile not found." });
        }

        return Ok(profile);
    }

    // ── PUT /api/profile ──────────────────────────────────────────────────────

    /// <summary>Updates the authenticated user's editable profile fields.</summary>
    [HttpPut]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var userId = GetUserId();
        var role   = GetRole();

        var updated = await _profileService.UpdateProfileAsync(userId, role, dto, ct);
        if (!updated)
        {
            _logger.LogWarning("Profile update target not found for {UserId} ({Role})", userId, role);
            return NotFound(new { error = "Profile not found." });
        }

        return NoContent();
    }

    // ── POST /api/profile/change-password ────────────────────────────────────

    /// <summary>Changes the authenticated user's password after verifying the current one.</summary>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var userId = GetUserId();
        var role   = GetRole();

        var result = await _profileService.ChangePasswordAsync(userId, role, dto, ct);

        return result switch
        {
            ChangePasswordResult.Success               => Ok(new { message = "Password changed successfully." }),
            ChangePasswordResult.WrongCurrentPassword  => UnprocessableEntity(new { error = "Current password is incorrect." }),
            ChangePasswordResult.SameAsCurrentPassword => UnprocessableEntity(new { error = "New password must differ from the current password." }),
            ChangePasswordResult.UserNotFound          => NotFound(new { error = "User not found." }),
            _                                          => StatusCode(500, new { error = "Unexpected error." })
        };
    }

    // ── POST /api/profile/photo ───────────────────────────────────────────────

    private static readonly HashSet<string> AllowedMimeTypes =
        new(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/webp" };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    /// <summary>
    /// Uploads a new profile photo for the authenticated user.
    /// Accepts multipart/form-data with field name "photo".
    /// Max size: 5 MB. Allowed types: image/jpeg, image/png, image/webp.
    /// </summary>
    [HttpPost("photo")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)] // slightly above MaxFileSizeBytes for overhead
    public async Task<IActionResult> UploadPhoto(
        IFormFile? photo, CancellationToken ct)
    {
        // ── 1. Validate file present ──────────────────────────────────────────
        if (photo is null || photo.Length == 0)
            return BadRequest(new { error = "No file uploaded. Include a 'photo' field." });

        // ── 2. Validate size ──────────────────────────────────────────────────
        if (photo.Length > MaxFileSizeBytes)
            return BadRequest(new { error = "File exceeds the 5 MB limit." });

        // ── 3. Validate MIME type — never trust extension alone ───────────────
        if (!AllowedMimeTypes.Contains(photo.ContentType))
            return BadRequest(new { error = "Unsupported file type. Allowed: JPEG, PNG, WebP." });

        var userId = GetUserId();
        var role   = GetRole();

        try
        {
            // ── 4. Read existing photo URL so we can clean up the old file later ──
            var existing = await _profileService.GetProfileAsync(userId, role, ct);
            var oldUrl   = existing?.PhotoUrl;

            // ── 5. Upload the new file ────────────────────────────────────────
            // Never pass the original file name to storage — use only the extension.
            var safeExt      = Path.GetExtension(photo.FileName);
            var safeFileName = $"{Guid.NewGuid():N}{safeExt}";

            await using var stream = photo.OpenReadStream();
            var newUrl = await _fileStorage.UploadAsync(stream, safeFileName, photo.ContentType, ct);

            // ── 6. Persist the URL ────────────────────────────────────────────
            await _profileService.UpdatePhotoUrlAsync(userId, role, newUrl, ct);

            // ── 7. Delete the old local file (ignore external/Azure URLs) ─────
            if (!string.IsNullOrWhiteSpace(oldUrl) &&
                Ssomero.Api.Services.Implementations.LocalFileStorageService.IsLocalPath(oldUrl))
            {
                await _fileStorage.DeleteAsync(oldUrl, ct);
            }

            _logger.LogInformation("Profile photo updated for {UserId}", userId);
            return Ok(new UploadPhotoResponse { PhotoUrl = newUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Photo upload failed for {UserId}", userId);
            return StatusCode(500, new { error = "Photo upload failed. Please try again." });
        }
    }
}


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssomero.Api.Controllers;

/// <summary>POST /api/upload/photo — stores a profile photo and returns the accessible URL.</summary>
[ApiController]
[Route("api/upload")]
[Authorize]
public class UploadController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<UploadController> _logger;

    // Allowed MIME types and their corresponding extensions
    private static readonly Dictionary<string, string> _allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"]  = ".png",
        ["image/webp"] = ".webp",
    };

    private const long MaxBytes = 5 * 1024 * 1024; // 5 MB

    public UploadController(IWebHostEnvironment env, ILogger<UploadController> logger)
    {
        _env    = env;
        _logger = logger;
    }

    [HttpPost("photo")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadPhoto(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No file provided." });

        if (file.Length > MaxBytes)
            return BadRequest(new { error = "File exceeds the 5 MB limit." });

        if (!_allowed.TryGetValue(file.ContentType, out var ext))
            return BadRequest(new { error = "Only JPEG, PNG and WebP images are accepted." });

        var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "photos");
        Directory.CreateDirectory(uploadsDir);

        var fileName  = $"{Guid.NewGuid()}{ext}";
        var filePath  = Path.Combine(uploadsDir, fileName);

        await using (var stream = System.IO.File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }

        var url = $"{Request.Scheme}://{Request.Host}/uploads/photos/{fileName}";
        _logger.LogInformation("Photo uploaded: {FileName}", fileName);
        return Ok(new { url });
    }
}

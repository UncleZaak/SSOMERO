using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AcademicSystem.Application.Common.Interfaces;

namespace AcademicSystem.Infrastructure.Services
{
    public class FileStorageLocal : IFileStorageService
    {
        private readonly string _basePath;
        private readonly ILogger<FileStorageLocal> _logger;

        public FileStorageLocal(ILogger<FileStorageLocal> logger)
        {
            _basePath = Path.Combine(Directory.GetCurrentDirectory(), "filestorage");
            Directory.CreateDirectory(_basePath);
            _logger = logger;
        }

        public async Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
        {
            var full = Path.Combine(_basePath, storagePath);
            if (File.Exists(full)) File.Delete(full);
            await Task.CompletedTask;
        }

        public async Task<Stream> GetAsync(string storagePath, CancellationToken cancellationToken = default)
        {
            var full = Path.Combine(_basePath, storagePath);
            return File.OpenRead(full);
        }

        public async Task<string> UploadAsync(Stream content, string contentType, string fileName, CancellationToken cancellationToken = default)
        {
            var full = Path.Combine(_basePath, fileName);
            using var fs = File.Create(full);
            await content.CopyToAsync(fs, cancellationToken);
            _logger.LogInformation("Stored file at {Path}", full);
            return fileName;
        }
    }
}

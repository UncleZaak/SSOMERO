using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicSystem.Application.Common.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadAsync(Stream content, string contentType, string fileName, CancellationToken cancellationToken = default);
        Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default);
        Task<Stream> GetAsync(string storagePath, CancellationToken cancellationToken = default);
    }
}

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Ssomero.Interfaces;

public interface IApiService
{
    Task<bool> CheckHealthAsync(CancellationToken ct = default);
    Task<HttpResponseMessage> GetAsync(string path, CancellationToken ct = default);
    Task<HttpResponseMessage> PostAsync(string path, HttpContent content, CancellationToken ct = default);
    Task<HttpResponseMessage> PutAsync(string path, HttpContent content, CancellationToken ct = default);
    Task<HttpResponseMessage> DeleteAsync(string path, CancellationToken ct = default);
}
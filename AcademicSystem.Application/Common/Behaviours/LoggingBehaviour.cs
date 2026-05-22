using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using AcademicSystem.Application.Common.Interfaces;

namespace AcademicSystem.Application.Common.Behaviours
{
public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    {
        private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;
        private readonly ICurrentUserService _currentUserService;

        public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger, ICurrentUserService currentUserService)
        {
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var userId = _currentUserService.UserId?.ToString() ?? "anonymous";

            _logger.LogInformation("Handling {RequestName} for user {UserId}", requestName, userId);

            var sw = Stopwatch.StartNew();
            var response = await next();
            sw.Stop();

            if (sw.ElapsedMilliseconds > 500)
            {
                _logger.LogWarning("Slow request {RequestName} ({ElapsedMilliseconds}ms)", requestName, sw.ElapsedMilliseconds);
            }

            _logger.LogInformation("Handled {RequestName} in {ElapsedMilliseconds}ms", requestName, sw.ElapsedMilliseconds);

            return response;
        }
    }
}

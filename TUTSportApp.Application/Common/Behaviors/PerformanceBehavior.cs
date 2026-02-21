using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace TUTSportApp.Application.Common.Behaviors
{
    public partial class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
        private readonly Stopwatch _timer;

        public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
            _timer = new Stopwatch();
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(next);
            _timer.Start();
            var response = await next().ConfigureAwait(false);
            _timer.Stop();

            if (_timer.ElapsedMilliseconds > 500)
            {
                LogLongRunningRequest(_logger, typeof(TRequest).Name, _timer.ElapsedMilliseconds, request!);
            }

            return response;
        }

        [LoggerMessage(EventId = 0, Level = LogLevel.Warning, Message = "Long Running Request: {Name} ({ElapsedMilliseconds} ms) {@Request}")]
        private static partial void LogLongRunningRequest(ILogger logger, string name, long elapsedMilliseconds, object request);
    }
}

using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace TUTSportApp.Application.Common.Behaviors
{
    public partial class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(next);
            LogHandlingRequest(_logger, typeof(TRequest).Name, request!);
            var response = await next().ConfigureAwait(false);
            LogHandledRequest(_logger, typeof(TRequest).Name, response!);
            return response;
        }

        [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Handling {RequestName} {@Request}")]
        private static partial void LogHandlingRequest(ILogger logger, string requestName, object request);

        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Handled {RequestName} {@Response}")]
        private static partial void LogHandledRequest(ILogger logger, string requestName, object response);
    }
}

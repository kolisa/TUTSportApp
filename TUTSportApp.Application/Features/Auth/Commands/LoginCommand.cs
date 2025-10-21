using MediatR;
using TUTSportApp.Application.Common.Interfaces;
using TUTSportApp.Application.Common.Models;

namespace TUTSportApp.Application.Features.Auth.Commands
{
    public record LoginCommand : IRequest<Result<string>>
    {
        public string Username { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<string>>
    {
        private readonly IAuthService _authService;
        private readonly ILoginRepository _loginRepository;

        public LoginCommandHandler(IAuthService authService, ILoginRepository loginRepository)
        {
            _authService = authService;
            _loginRepository = loginRepository;
        }

        public async Task<Result<string>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var login = await _loginRepository
                .GetByUsernameAsync(request.Username /*, cancellationToken*/)
                .ConfigureAwait(false);

            if (login is null)
            {
                return Result.Failure<string>("Invalid credentials");
            }

            if (login.IsLocked)
            {
                return Result.Failure<string>("Account is locked");
            }

            if (!_authService.VerifyPasswordHash(request.Password, login.PasswordHash))
            {
                await _loginRepository
                    .UpdateFailedAttemptsAsync(login.Id, login.FailedAttempts + 1 /*, cancellationToken*/)
                    .ConfigureAwait(false);

                return Result.Failure<string>("Invalid credentials");
            }

            var token = await _authService
                .CreateTokenAsync(request /*, cancellationToken*/)
                .ConfigureAwait(false);

            return Result.Success(token); // T inferred as string
        }
    }
}

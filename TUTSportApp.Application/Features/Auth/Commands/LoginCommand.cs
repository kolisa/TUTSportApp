

using AutoMapper;

using MediatR;
using TUTSportApp.Application.Common.Models;
using TUTSportApp.Domain.Common.Interfaces;
using TUTSportApp.Domain.Models;

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
        private readonly IMapper _mapper;
        public LoginCommandHandler(IAuthService authService, ILoginRepository loginRepository, IMapper mapper)
        {
            _authService = authService;
            _loginRepository = loginRepository;
            _mapper = mapper;

        }

        public async Task<Result<string>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var login = await _loginRepository
                .GetByUsernameAsync(request.Username)
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
                    .UpdateFailedAttemptsAsync(login.Id, login.FailedAttempts + 1)
                    .ConfigureAwait(false);

                return Result.Failure<string>("Invalid credentials");
            }

            var loginModel = _mapper.Map<LoginModel>(request);

            var token = await _authService
                .CreateTokenAsync(loginModel)
                .ConfigureAwait(false);

            return Result.Success(token); // T inferred as string
        }
    }
}

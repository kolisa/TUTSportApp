using System.ComponentModel.DataAnnotations;

using AutoMapper;

using MediatR;
using TUTSportApp.Application.Common.Models;
using TUTSportApp.Domain.Common.Interfaces;
using TUTSportApp.Domain.Models;

namespace TUTSportApp.Application.Features.Auth.Commands
{
    public record LoginCommand : IRequest<Result<string>>, IValidatableObject
    {
        [Required(ErrorMessage = "Username or email is required.")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters.")]
        [MaxLength(256, ErrorMessage = "Username or email cannot exceed 256 characters.")]
        public string Username { get; init; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
        public string Password { get; init; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                yield break;
            }

            // If it looks like an email, validate as email.
            if (Username.Contains('@', StringComparison.Ordinal))
            {
                var emailAttr = new EmailAddressAttribute();
                if (!emailAttr.IsValid(Username))
                {
                    yield return new ValidationResult(
                        "Invalid email format.",
                        new[] { nameof(Username) });
                }
            }
        }
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

            var mapper = _mapper.Map<LoginModel>(request);
            
            var token = await _authService
                .CreateTokenAsync(mapper /*, cancellationToken*/)
                .ConfigureAwait(false);

            return Result.Success(token); // T inferred as string
        }
    }
}

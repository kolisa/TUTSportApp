using FluentAssertions;
using Moq;
using TUTSportApp.Application.Features.Auth.Commands;
using TUTSportApp.Domain.Common.Interfaces;
using TUTSportApp.Domain.Models;
using TUTSportApp.Domain.Entities;
using AutoMapper;
using Xunit;
using System.Threading;
using System.Threading.Tasks;

namespace TUTSportApp.UnitTest.Features.Auth.Commands
{
    public class LoginCommandHandlerTests
    {
        private readonly Mock<IAuthService> _authServiceMock = new();
        private readonly Mock<ILoginRepository> _loginRepositoryMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly LoginCommandHandler _handler;

        public LoginCommandHandlerTests()
        {
            _handler = new LoginCommandHandler(_authServiceMock.Object, _loginRepositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task ShouldReturnFailureWhenUserNotFound()
        {
            _loginRepositoryMock.Setup(r => r.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((Login?)null);
            var command = new LoginCommand { Username = "nouser", Password = "pass" };
            var result = await _handler.Handle(command, CancellationToken.None);
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldReturnFailureWhenAccountIsLocked()
        {
            _loginRepositoryMock.Setup(r => r.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync(new Login { IsLocked = true });
            var command = new LoginCommand { Username = "user", Password = "pass" };
            var result = await _handler.Handle(command, CancellationToken.None);
            result.IsSuccess.Should().BeFalse();
        }
    }
}

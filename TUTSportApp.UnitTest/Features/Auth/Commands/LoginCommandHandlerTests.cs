using FluentAssertions;
using Moq;
using TUTSportApp.Application.Features.Auth.Commands;
using TUTSportApp.Domain.Common.Interfaces;
using TUTSportApp.Domain.Models;
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
        public async Task Should_Return_Failure_When_User_Not_Found()
        {
            _loginRepositoryMock.Setup(r => r.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((Login)null);
            var command = new LoginCommand { Username = "nouser", Password = "pass" };
            var result = await _handler.Handle(command, CancellationToken.None);
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Should_Return_Failure_When_Account_Is_Locked()
        {
            _loginRepositoryMock.Setup(r => r.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync(new Login { IsLocked = true });
            var command = new LoginCommand { Username = "user", Password = "pass" };
            var result = await _handler.Handle(command, CancellationToken.None);
            result.IsSuccess.Should().BeFalse();
        }
    }
}

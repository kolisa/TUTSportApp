using FluentAssertions;
using TUTSportApp.Application.Features.Auth.Commands;
using Xunit;

namespace TUTSportApp.UnitTest.Features.Auth.Commands
{
    public class LoginCommandModelTests
    {
        [Fact]
        public void ShouldSetUsernameAndPassword()
        {
            var command = new LoginCommand { Username = "user1", Password = "pass1" };
            command.Username.Should().Be("user1");
            command.Password.Should().Be("pass1");
        }
    }
}

using FluentAssertions;
using TUTSportApp.Application.Features.Auth.Commands;
using Xunit;

namespace TUTSportApp.UnitTest.Features.Auth.Commands
{
    public class LoginCommandValidatorTests
    {
        private readonly LoginCommandValidator _validator = new();

        [Fact]
        public void UsernameEmptyHasValidationError()
        {
            var result = _validator.Validate(new LoginCommand { Username = "", Password = "password123" });
            result.Errors.Should().Contain(x => x.PropertyName == "Username");
        }

        [Fact]
        public void UsernameWhitespaceHasValidationError()
        {
            var result = _validator.Validate(new LoginCommand { Username = "   ", Password = "password123" });
            result.Errors.Should().Contain(x => x.PropertyName == "Username");
        }

        [Fact]
        public void UsernameExceedsMaxLengthHasValidationError()
        {
            var result = _validator.Validate(new LoginCommand { Username = new string('a', 51), Password = "password123" });
            result.Errors.Should().Contain(x => x.PropertyName == "Username");
        }

        [Fact]
        public void UsernameAtMaxLengthNoValidationError()
        {
            var result = _validator.Validate(new LoginCommand { Username = new string('a', 50), Password = "password123" });
            result.Errors.Should().NotContain(x => x.PropertyName == "Username");
        }

        [Fact]
        public void UsernameValidNoValidationError()
        {
            var result = _validator.Validate(new LoginCommand { Username = "validuser", Password = "password123" });
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void PasswordEmptyHasValidationError()
        {
            var result = _validator.Validate(new LoginCommand { Username = "user", Password = "" });
            result.Errors.Should().Contain(x => x.PropertyName == "Password");
        }

        [Fact]
        public void PasswordTooShortHasValidationError()
        {
            var result = _validator.Validate(new LoginCommand { Username = "user", Password = "123" });
            result.Errors.Should().Contain(x => x.PropertyName == "Password");
        }

        [Fact]
        public void PasswordAtMinLengthNoValidationError()
        {
            var result = _validator.Validate(new LoginCommand { Username = "user", Password = new string('a', 6) });
            result.Errors.Should().NotContain(x => x.PropertyName == "Password");
        }

        [Fact]
        public void PasswordValidNoValidationError()
        {
            var result = _validator.Validate(new LoginCommand { Username = "user", Password = "password123" });
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void BothFieldsEmptyHasTwoValidationErrors()
        {
            var result = _validator.Validate(new LoginCommand { Username = "", Password = "" });
            result.Errors.Should().Contain(x => x.PropertyName == "Username");
            result.Errors.Should().Contain(x => x.PropertyName == "Password");
        }
    }
}

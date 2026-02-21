using FluentAssertions;
using TUTSportApp.Application.Features.Auth.Commands;
using Xunit;

namespace TUTSportApp.UnitTest.Features.Auth.Commands
{
    public class LoginCommandValidatorTests
    {
        private readonly LoginCommandValidator _validator = new();

        [Fact]
        public void Username_Empty_HasValidationError()
        {
            var result = _validator.Validate(new LoginCommand { Username = "", Password = "password123" });
            result.Errors.Should().Contain(x => x.PropertyName == "Username");
        }

        [Fact]
        public void Username_Whitespace_HasValidationError()
        {
            var result = _validator.Validate(new LoginCommand { Username = "   ", Password = "password123" });
            result.Errors.Should().Contain(x => x.PropertyName == "Username");
        }

        [Fact]
        public void Username_ExceedsMaxLength_HasValidationError()
        {
            var result = _validator.Validate(new LoginCommand { Username = new string('a', 51), Password = "password123" });
            result.Errors.Should().Contain(x => x.PropertyName == "Username");
        }

        [Fact]
        public void Username_AtMaxLength_NoValidationError()
        {
            var result = _validator.Validate(new LoginCommand { Username = new string('a', 50), Password = "password123" });
            result.Errors.Should().NotContain(x => x.PropertyName == "Username");
        }

        [Fact]
        public void Username_Valid_NoValidationError()
        {
            var result = _validator.Validate(new LoginCommand { Username = "validuser", Password = "password123" });
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Password_Empty_HasValidationError()
        {
            var result = _validator.Validate(new LoginCommand { Username = "user", Password = "" });
            result.Errors.Should().Contain(x => x.PropertyName == "Password");
        }

        [Fact]
        public void Password_TooShort_HasValidationError()
        {
            var result = _validator.Validate(new LoginCommand { Username = "user", Password = "123" });
            result.Errors.Should().Contain(x => x.PropertyName == "Password");
        }

        [Fact]
        public void Password_AtMinLength_NoValidationError()
        {
            var result = _validator.Validate(new LoginCommand { Username = "user", Password = new string('a', 6) });
            result.Errors.Should().NotContain(x => x.PropertyName == "Password");
        }

        [Fact]
        public void Password_Valid_NoValidationError()
        {
            var result = _validator.Validate(new LoginCommand { Username = "user", Password = "password123" });
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void BothFieldsEmpty_HasTwoValidationErrors()
        {
            var result = _validator.Validate(new LoginCommand { Username = "", Password = "" });
            result.Errors.Should().Contain(x => x.PropertyName == "Username");
            result.Errors.Should().Contain(x => x.PropertyName == "Password");
        }
    }
}

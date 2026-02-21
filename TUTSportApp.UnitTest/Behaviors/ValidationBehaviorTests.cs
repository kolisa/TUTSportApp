using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Moq;
using TUTSportApp.Application.Common.Behaviors;
using Xunit;

namespace TUTSportApp.UnitTest.Behaviors
{
    public class ValidationBehaviorTests
    {
        private sealed record TestRequest(string Value) : IRequest<string>;

        private sealed class TestValidator : AbstractValidator<TestRequest>
        {
            public TestValidator()
            {
                RuleFor(x => x.Value).NotEmpty();
            }
        }

        [Fact]
        public async Task HandleNoValidatorsCallsNextDelegate()
        {
            var behavior = new ValidationBehavior<TestRequest, string>(Array.Empty<IValidator<TestRequest>>());
            var called = false;
            await behavior.Handle(new TestRequest("foo"), () => { called = true; return Task.FromResult("ok"); }, CancellationToken.None);
            Assert.True(called);
        }

        [Fact]
        public async Task HandleAllValidatorsPassCallsNextDelegate()
        {
            var validator = new TestValidator();
            var behavior = new ValidationBehavior<TestRequest, string>(new[] { validator });
            var called = false;
            await behavior.Handle(new TestRequest("foo"), () => { called = true; return Task.FromResult("ok"); }, CancellationToken.None);
            Assert.True(called);
        }

        [Fact]
        public async Task HandleValidatorFailsThrowsValidationException()
        {
            var validator = new TestValidator();
            var behavior = new ValidationBehavior<TestRequest, string>(new[] { validator });
            await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
                behavior.Handle(new TestRequest(""), () => Task.FromResult("fail"), CancellationToken.None));
        }

        [Fact]
        public async Task HandleValidatorFailsDoesNotCallNextDelegate()
        {
            var validator = new TestValidator();
            var behavior = new ValidationBehavior<TestRequest, string>(new[] { validator });
            var called = false;
            await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
                behavior.Handle(new TestRequest(""), () => { called = true; return Task.FromResult("fail"); }, CancellationToken.None));
            Assert.False(called);
        }
    }
}

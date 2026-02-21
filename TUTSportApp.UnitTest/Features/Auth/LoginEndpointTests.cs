using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using TUTSportApp.Application.Features.Auth.Commands;
using Xunit;
using TUTSportApp.UnitTest.Infrastructure;

namespace TUTSportApp.UnitTest.Features.Auth
{
    public sealed class LoginEndpointTests : IClassFixture<IntegrationTestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public LoginEndpointTests(IntegrationTestWebApplicationFactory factory)
        {
            ArgumentNullException.ThrowIfNull(factory);
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task LoginWithValidCredentialsReturnsJwtToken()
        {
            var command = new LoginCommand { Username = "validuser", Password = "validpassword" };
            var response = await _client.PostAsJsonAsync("/api/login/login", command);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>() ?? new LoginResponse { Token = string.Empty };
            result.Token.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task LoginWithInvalidCredentialsReturnsUnauthorized()
        {
            var command = new LoginCommand { Username = "invaliduser", Password = "wrongpassword" };
            var response = await _client.PostAsJsonAsync("/api/login/login", command);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        private sealed class LoginResponse
        {
            public string Token = string.Empty;
        }
    }
}

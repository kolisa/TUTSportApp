using Xunit;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TUTSportApp.IntegrationTest.Auth
{
    [Collection("IntegrationTestCollection")]
    public class LoginEndpointTests
    {
        private readonly HttpClient _client;

        public LoginEndpointTests(Infrastructure.IntegrationTestWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_ForInvalidCredentials()
        {
            // Arrange
            var content = new StringContent("{\"username\":\"baduser\",\"password\":\"badpass\"}", System.Text.Encoding.UTF8, "application/json");
            // Act
            var response = await _client.PostAsync("/api/login", content);
            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
// using Microsoft.EntityFrameworkCore; // Removed duplicate
using TUTSportApp.Infrastructure.Data.Context;
using TUTSportApp.Api;
using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.InMemory; // Removed, not needed

namespace TUTSportApp.UnitTest.Infrastructure
{
    public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>
    {


        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.ConfigureServices(services =>
            {
                // Remove the app's ApplicationDbContext registration.
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                // Add ApplicationDbContext using InMemory database for testing.
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb-" + Guid.NewGuid()); // Ensure correct using
                });
            });
        }
    }
}
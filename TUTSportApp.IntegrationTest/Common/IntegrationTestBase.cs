using Xunit;
using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using TUTSportApp.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using TUTSportApp.Domain.Common.Interfaces;

namespace TUTSportApp.IntegrationTest.Common
{
    public abstract class IntegrationTestBase : IDisposable
    {
        protected readonly HttpClient Client;
        protected readonly ApplicationDbContext DbContext;
        protected readonly IAuthService AuthService;
        private readonly IServiceScope _scope;
        private readonly IServiceProvider _provider;

        protected IntegrationTestBase(Infrastructure.IntegrationTestWebApplicationFactory factory)
        {
            Client = factory.CreateClient();
            _provider = factory.Services;
            _scope = _provider.CreateScope();
            DbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            AuthService = _scope.ServiceProvider.GetRequiredService<IAuthService>();
        }

        public void Dispose()
        {
            _scope.Dispose();
            Client.Dispose();
        }
    }
}

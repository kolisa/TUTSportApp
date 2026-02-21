using Xunit;

namespace TUTSportApp.IntegrationTest.Common
{
    [CollectionDefinition("IntegrationTestCollection")]
    public class IntegrationTestCollection : ICollectionFixture<Infrastructure.IntegrationTestWebApplicationFactory>
    {
    }
}

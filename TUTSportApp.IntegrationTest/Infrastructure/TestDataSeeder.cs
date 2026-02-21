using Bogus;
using TUTSportApp.Domain.Entities;
using TUTSportApp.Infrastructure.Data.Context;

namespace TUTSportApp.IntegrationTest.Infrastructure
{
    public static class TestDataSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            var userFaker = new Faker<User>()
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.Username, f => f.Internet.UserName())
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.PasswordHash, f => f.Internet.Password());

            var users = userFaker.Generate(10);
            context.Users.AddRange(users);
            context.SaveChanges();
        }
    }
}
